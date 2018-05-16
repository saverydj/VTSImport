using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;

namespace STARS.Applications.VETS.Plugins.VTS.Interface
{
    [Export(typeof(VTS))]
    public class VTS
    {
        const string IllegalChars = "~-\\!@#$%^()&*+=|{}[]\"'<>?/:,_�";
        private List<Field> _fieldMap = new List<Field>();
        private List<string> _illegalTests = new List<string>();

        #region Repository

        internal void UpdateRepsitory(bool showWindow)
        {
            CheckFolderPath(Config.TempFolder);
            CopyXMLTemplate(Config.VtsRepository);
            PopulateFieldMap();
            FormatSchedule();
            ParseSchedule();
            if (Config.InformUserOfInvalidTests) ProvideUserFeedback();
        }

        private void WriteToRepository(ref XDocument vtsRepository)
        {
            var tests = vtsRepository.Descendants().Where(e => e.Name.LocalName == "VtsTests").First();
            var test = new XElement("VtsTest");
            XElement resource;
            foreach (Field field in _fieldMap)
            {
                if (!String.IsNullOrEmpty(field.VetsTag) && !String.IsNullOrEmpty(field.ResourceType))
                {
                    if (test.Descendants().Where(e => e.Name.LocalName == field.ResourceType).Any())
                    {
                        resource = test.Descendants().Where(e => e.Name.LocalName == field.ResourceType).First();
                    }
                    else
                    {
                        resource = new XElement(field.ResourceType);
                        test.Add(resource);
                    }
                    resource.Add(new XElement(field.VetsTag, field.FieldValue));
                }
            }
            test = new XElement("VtsTest", test.Elements().OrderBy(e => e.Name.LocalName));
            tests.Add(test);
        }

        #endregion

        #region FieldMap

        private void PopulateFieldMap()
        {
            XDocument fieldMapConfig = XDocument.Load(Config.FieldMap);
            IEnumerable<XElement> entries = fieldMapConfig.Descendants().Where(e => e.Name.LocalName == "Entry");
            string vts;
            string vets;
            string resource;
            _fieldMap.Clear();

            foreach (XElement entry in entries)
            {
                try
                {
                    vts = entry.Descendants("VTSTag").First().Value;
                    vets = entry.Descendants("VETSTag").First().Value;
                    resource = entry.Descendants("VETSResourceType").First().Value;
                    _fieldMap.Add(new Field(vts, vets, resource));
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + " Failed on fieldMap entry: " + entry);
                }
            }
        }

        private void ClearFieldMap()
        {
            foreach (Field field in _fieldMap)
            {
                field.FieldValue = String.Empty;
            }
        }

        public Field GetMappedField(string tag)
        {
            foreach (Field field in _fieldMap)
            {
                if (field.VetsTag == tag || field.VtsTag == tag) return field;
            }
            return null;
        }

        public string GetMappedFieldValue(string tag)
        {
            foreach (Field field in _fieldMap)
            {
                if (field.VetsTag == tag || field.VtsTag == tag) return field.FieldValue;
            }
            return null;
        }

        public void SetMappedFieldValue(string tag, string value)
        {
            foreach (Field field in _fieldMap)
            {
                if (field.VetsTag == tag || field.VtsTag == tag) field.FieldValue = value;
            }
        }

        public void ModifyMappedFieldValue(string tag, double mod)
        {
            foreach (Field field in _fieldMap)
            {
                if (field.VetsTag == tag || field.VtsTag == tag)
                {
                    field.FieldValue = (TypeCast.ToDouble(field.FieldValue) * mod).ToString();
                }
            }
        }

        #endregion

        #region DaySchedule

        private void FormatSchedule()
        {
            string[] vstRawData = File.ReadAllLines(Config.VtsSchedule);
            List<string> vtsFormattedData = new List<string>();
            int index;
            string formattedField;

            for (int i = 0; i < vstRawData.Length; i++)
            {
                try
                {
                    formattedField = "";
                    index = vstRawData[i].TakeWhile(c => char.IsWhiteSpace(c)).Count();
                    if (index < vstRawData[i].Length && vstRawData[i][index] == '<')
                    {
                        if (vstRawData[i].Contains("</") || vstRawData[i].Contains("/>") || vstRawData[i + 1].Contains("<")) vtsFormattedData.Add(vstRawData[i]);
                        else
                        {
                            do
                            {
                                formattedField += vstRawData[i].TrimEnd();
                                if (!formattedField.EndsWith(">")) formattedField += " ";
                                i++;
                            }
                            while (!vstRawData[i - 1].Contains("</"));
                            i--;
                            vtsFormattedData.Add(formattedField);
                        }
                    }
                }
                catch { }
            }
            File.WriteAllLines(Config.VtsFormattedData, vtsFormattedData.ToArray(), Encoding.Unicode);
        }

        private void ParseSchedule()
        {
            _illegalTests.Clear();

            XDocument vtsScedule = XDocument.Load(Config.VtsFormattedData);
            XDocument vtsRepository = XDocument.Load(Config.VtsRepository);
            IEnumerable<XElement> vtsTests = vtsScedule.Descendants().Where(e => e.Name.LocalName == "ROW");

            foreach (XElement vtsTest in vtsTests)
            {
                foreach (XElement field in vtsTest.Descendants())
                {
                    SetMappedFieldValue(field.Name.LocalName, field.Value == Config.NullValueKey ? String.Empty : field.Value);
                }

                if (!IsValidTestCode(GetMappedFieldValue("TestTypeCode")))
                {
                    _illegalTests.Add(GetMappedFieldValue("TestIDNumber"));
                }
                else if (GetMappedFieldValue("TestStatus") == "P")
                {
                    UniqueFieldModification();
                    if(Config.CheckThresholdValues) CheckFieldThresholds();
                    WriteToRepository(ref vtsRepository);
                    vtsRepository.Save(Config.VtsRepository);
                }
                ClearFieldMap();
            }
        }

        #endregion

        #region UniqueFieldModification

        private void UniqueFieldModification()
        {
            try
            {
                ModificationsFromFile();
            }
            catch(Exception ex)
            {
                throw new Exception(String.Format("Error reading {0}. {1}", Config.UniqueFieldModifications, ex.Message));
            }         

            SetMappedFieldValue("FuelName", Config.FuelResourceName);
            SetMappedFieldValue("VehicleName", Config.VehicleResourceName);
            SetMappedFieldValue("SamplingConfigurationName", Config.SamplingConfigurationResourceName);
            SetMappedFieldValue("SampleLineConfigurationName", Config.SampleLineConfigurationResourceName);
            SetMappedFieldValue("IOChannelsSetupName", Config.IOChannelsSetupResourceName);
        }

        private void ModificationsFromFile()
        {
            XDocument text = XDocument.Load(Config.UniqueFieldModifications);
            IEnumerable<XElement> nodes = text.Descendants().Where(e => e.Name.LocalName == "Case");
            foreach (XElement node in nodes)
            {
                XElement ifNode = node.Descendants().Where(e => e.Name.LocalName == "If").FirstOrDefault();
                IEnumerable<XElement> elseifNodes = node.Descendants().Where(e => e.Name.LocalName == "ElseIf");
                XElement elseNode = node.Descendants().Where(e => e.Name.LocalName == "Else").FirstOrDefault();
                bool conditionMet = false;

                if (!conditionMet && ifNode != null) conditionMet = EvaluateExpression(ifNode);
                foreach (XElement elseifNode in elseifNodes)
                {
                    if (!conditionMet && elseifNode != null) conditionMet = EvaluateExpression(elseifNode);
                }
                if (!conditionMet && elseNode != null) EnactStatement(elseNode.Value);
            }
        }

        private bool EvaluateExpression(XElement toEvaluate)
        {
            XElement condition = toEvaluate.Descendants().Where(e => e.Name.LocalName == "Condition").FirstOrDefault();
            XElement then = toEvaluate.Descendants().Where(e => e.Name.LocalName == "Then").FirstOrDefault();
            if (condition == null || then == null)
            {
                throw new Exception(String.Format("{0} must contains both <Condition></Condition> and <Then></Then> tags.", toEvaluate));
            }

            string conditionString = condition.Value;
            ReplaceFieldNameWithValue(ref conditionString);
            List<PartExpression> partExpressions = new List<PartExpression>();
            PartExpression partExpression = new PartExpression();
            foreach (string segment in conditionString.Split(' '))
            {
                if (segment == "&&" || segment == "||")
                {
                    partExpression.EvaluateExpression();
                    partExpressions.Add(partExpression);
                    partExpression = new PartExpression();
                    partExpression.LogicalOperator = segment;
                }
                else if (segment == "==" || segment == ">" || segment == "<" || segment == ">=" || segment == "<=" || segment == "!=") partExpression.ComparativeOperator = segment;
                else partExpression.SetValue(segment);
            }
            partExpression.EvaluateExpression();
            partExpressions.Add(partExpression);

            bool conditionSatisfied = false;
            foreach (PartExpression thisPartExpression in partExpressions)
            {
                if (thisPartExpression.LogicalOperator == null) conditionSatisfied = thisPartExpression.Evaluated;
                else if (thisPartExpression.LogicalOperator == "&&") conditionSatisfied = conditionSatisfied && thisPartExpression.Evaluated;
                else if (thisPartExpression.LogicalOperator == "||") conditionSatisfied = conditionSatisfied || thisPartExpression.Evaluated;
            }

            if (conditionSatisfied) EnactStatement(then.Value);
            return conditionSatisfied;
        }

        private void EnactStatement(string allToDo)
        {
            string leftside;
            string rightside;
            foreach (string thisToDo in allToDo.Replace("\t", "").Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!thisToDo.Contains('='))
                {
                    throw new Exception(String.Format("The expression \"{0}\" could not be evaluated because no '=' is present.", thisToDo));
                }
                else if (thisToDo.Split('=').Length - 1 > 1)
                {
                    throw new Exception(String.Format("The expression \"{0}\" could not be evaluated because more than one instance of '=' is present.", thisToDo));
                }

                leftside = thisToDo.Split(new string[] { "=" }, StringSplitOptions.None)[0].Trim();
                rightside = thisToDo.Split(new string[] { "=" }, StringSplitOptions.None)[1].Trim();
                if (!leftside.StartsWith("@"))
                {
                    throw new Exception(String.Format("The expression \"{0}\" could not be evaluated because it does not begin with '@'.", thisToDo));
                }
                else if (leftside.Split('@').Length - 1 > 1)
                {
                    throw new Exception(String.Format("The expression \"{0}\" could not be evaluated because more than one instance of '@' is present on the left hand side of the '=' operator.", thisToDo));
                }
                leftside = leftside.Substring(1, leftside.Length - 1);

                if (rightside.Contains("+") || rightside.Contains("-") || rightside.Contains("*") || rightside.Contains("/") && !rightside.Contains("\""))
                {
                    ReplaceFieldNameWithValue(ref rightside, true);
                    SolveMathematicalExpression(ref rightside);
                }
                else ReplaceFieldNameWithValue(ref rightside);
                rightside = rightside.Replace("\"", "");
                SetMappedFieldValue(leftside, rightside);
            }
        }

        public void SolveMathematicalExpression(ref string expression)
        {
            DataTable table = new DataTable();
            table.Columns.Add("expression", typeof(string), expression);
            DataRow row = table.NewRow();
            table.Rows.Add(row);
            expression = double.Parse((string)row["expression"]).ToString();
        }

        private void ReplaceFieldNameWithValue(ref string inputString, bool isDouble = false)
        {
            string outputString = inputString;
            string name = String.Empty;
            bool takeName = false;
            for (int j = 0; j < inputString.Length; j++)
            {
                if (takeName && inputString[j] != ' ' && !IllegalChars.Replace("_", "").Contains(inputString[j]))
                {
                    name += inputString[j];
                }
                if (inputString[j] == ' ' || IllegalChars.Replace("_", "").Contains(inputString[j]) || j == inputString.Length - 1)
                {
                    if (takeName)
                    {
                        if (isDouble) outputString = outputString.Replace("@" + name, TypeCast.ToDouble(GetMappedFieldValue(name)).ToString());
                        else outputString = outputString.Replace("@" + name, GetMappedFieldValue(name));
                    }
                    takeName = false;
                }
                if (inputString[j] == '@')
                {
                    takeName = true;
                    name = String.Empty;
                }
            }
            inputString = outputString;
        }

        #endregion

        #region VTS-VETS

        private bool IsValidTestCode(string vtsCode)
        {
            XDocument text = XDocument.Load(Config.ValidTestNames);
            IEnumerable<XElement> nodes = text.Descendants().Where(e => e.Name.LocalName == "Test");
            foreach (XElement node in nodes)
            {
                if (node.Descendants().Where(e => e.Name.LocalName == "VTSTestCode").First().Value == vtsCode)
                {
                    IEnumerable<XElement> vetsResources = node.Descendants().Where(e => e.Name.LocalName == "VETSResources").First().Descendants();
                    foreach (XElement vetsResource in vetsResources)
                    {
                        SetMappedFieldValue(vetsResource.Name.ToString(), vetsResource.Value);
                    }
                    return true;
                }
            }
            return false;
        }
      
        private void CheckFieldThresholds()
        {
            List<string> InvalidFields = new List<string>();
            XDocument text = XDocument.Load(Config.FieldThresholds);
            IEnumerable<XElement> nodes = text.Descendants().Where(e => e.Name.LocalName == "Field");
            Field field;
            string low;
            string high;
            foreach (XElement node in nodes)
            {
                field = GetMappedField(node.Descendants().Where(e => e.Name.LocalName == "Name").First().Value);
                low = node.Descendants().Where(e => e.Name.LocalName == "Low").First().Value;
                high = node.Descendants().Where(e => e.Name.LocalName == "High").First().Value;
                if(TypeCast.IsDouble(low) && TypeCast.ToDouble(field.FieldValue) < TypeCast.ToDouble(low))
                {
                    field.FieldValue = low;
                    InvalidFields.Add(field.VetsTag);
                }
                else if(TypeCast.IsDouble(high) && TypeCast.ToDouble(field.FieldValue) > TypeCast.ToDouble(high))
                {
                    field.FieldValue = high;
                    InvalidFields.Add(field.VetsTag);
                }
            }
            if(InvalidFields.Count > 0 && Config.InformUserOfFieldsOutsideThreshold)
            {
                string fieldNames = "";
                foreach (string fieldName in InvalidFields)
                {
                    if (fieldNames != "") fieldNames += ", ";
                    fieldNames += fieldName;
                }
                MessageBox.Show
                (
                    new Form { TopMost = true }, 
                    String.Format("VTS test {0}. The following fields were found to be outside of " +
                    "acceptable threshold range and were subsequently modified: {1}", GetMappedFieldValue("TestIDNumber"), fieldNames)
                    , "VTS Data Import", MessageBoxButtons.OK, MessageBoxIcon.Warning
                );
            }
        }

        private void FormatVETSNames(string vetsTestName)
        {
            FormatVETSName(ref vetsTestName, false);        
            SetMappedFieldValue("VETS_TEST_NAME", vetsTestName);
        }

        private void FormatVETSName(ref string fieldValue)
        {
            FormatVETSName(ref fieldValue, false);
        }

        private void FormatVETSName(ref string fieldValue, bool vtsKeyWord)
        {
            fieldValue = Regex.Replace(fieldValue, @"[^\u0000-\u007F]+", "�");
            string buildOutput = "";
            bool isCharIllegal = false;

            for (int i = 0; i < fieldValue.Length; i++)
            {
                for (int j = 0; j < IllegalChars.Length; j++)
                {
                    if (fieldValue[i] == IllegalChars[j])
                    {
                        isCharIllegal = true;
                        break;
                    }
                }
                if (!isCharIllegal)
                {
                    buildOutput += fieldValue[i];
                }
                else
                {
                    if (i == 0)
                    {
                        if (fieldValue.Length > 1 && fieldValue[i + 1] == ' ')
                        {
                            i++;
                        }
                    }
                    else if (i < fieldValue.Length - 1)
                    {
                        if (fieldValue[i - 1] == ' ' && fieldValue[i + 1] == ' ')
                        {
                            i++;
                        }
                        else if (fieldValue[i - 1] != ' ' && fieldValue[i + 1] != ' ')
                        {
                            buildOutput += " ";
                        }
                    }
                    isCharIllegal = false;
                }
            }

            if (buildOutput.Length == 0) buildOutput = Guid.NewGuid().ToString();

            while (buildOutput[buildOutput.Length - 1] == '.' || buildOutput[buildOutput.Length - 1] == ' ')
            {
                buildOutput = buildOutput.Trim();
                buildOutput = buildOutput.TrimEnd('.');
                if (buildOutput.Length == 0) buildOutput = Guid.NewGuid().ToString();
            }

            if (buildOutput.Length > 0 && (vtsKeyWord || Char.IsNumber(buildOutput[0])))
            {
                buildOutput = "VTS " + buildOutput;
            }

            fieldValue = buildOutput;

        }

        #endregion

        #region XML

        private string GetXMLField(XDocument text, string tag, string parentTag)
        {
            var nodes = text.Descendants().Where(e => e.Name.LocalName == tag);
            if (nodes.Count() == 0)
            {
                nodes = text.Descendants().Where(e => e.Name.LocalName == "CustomFieldID");
                if (nodes.Count() > 0)
                {
                    foreach (var node in nodes)
                    {
                        if (node.Value.ToString() == tag)
                        {
                            XElement valueNode = (XElement)node.NextNode;
                            return valueNode.Value.ToString();
                        }
                    }
                }
            }
            else if (nodes.Count() == 1)
            {
                foreach (var node in nodes)
                {
                    return node.Value.ToString();
                }
            }
            else if (nodes.Count() > 1)
            {
                foreach (var node in nodes)
                {
                    if (parentTag == "" || node.Parent.Name.LocalName == parentTag)
                    {
                        return node.Value.ToString();
                    }
                }
            }
            return null;
        }

        private void ReplaceXMLField(ref XDocument text, string tag, string parentTag, string field)
        {
            var nodes = text.Descendants().Where(e => e.Name.LocalName == tag);
            if (nodes.Count() == 0)
            {
                nodes = text.Descendants().Where(e => e.Name.LocalName == "CustomFieldID");
                if (nodes.Count() > 0)
                {
                    foreach (var node in nodes)
                    {
                        if (node.Value.ToString() == tag)
                        {
                            XElement valueNode = (XElement)node.NextNode;
                            valueNode.SetValue(field);
                        }
                    }
                }
            }
            else if (nodes.Count() == 1)
            {
                foreach (var node in nodes)
                {
                    node.SetValue(field);
                }
            }
            else if (nodes.Count() > 1)
            {
                foreach (var node in nodes)
                {
                    if (parentTag == "" || node.Parent.Name.LocalName == parentTag)
                    {
                        node.SetValue(field);
                    }
                }
            }
        }

        #endregion

        #region System.IO

        private void CopyXMLTemplate(string path)
        {
            File.WriteAllBytes(path, Properties.Resources.xmlTemplate);
        }

        private void CheckFolderPath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        #endregion

        #region Dialog

        private void ProvideUserFeedback()
        {
            string testIDs = "";

            if (_illegalTests.Count > 0)
            {
                testIDs = "";
                foreach (string id in _illegalTests)
                {
                    if (testIDs != "") testIDs += ", ";
                    testIDs += id;
                }
                MessageBox.Show(new Form { TopMost = true }, "The test type of the following tests were found to be invalid. Tests with ID numbers as follows were not imported into VETS: " + testIDs + ".", "VTS Data Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

    }

}

