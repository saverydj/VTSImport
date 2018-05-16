3 dlls and 1 config file from "DLL" folder goes to "C:\Program Files (x86)\SRH Systems\STARS\VETS" with other VETS dlls

Open VETS and Import: Custom Enumerations, Custom Fields, UI Configurations, VTS Fuel, VTS IO Channel Setup, VTS Sample Line Configuration, VTS Sampling Configuration, VTS Test, and VTS Vehicle. If any of the imported resources do not validate you may delete the invalid resource and create a new resource of that type with the same name. 

Copy paste SharedFolder somewhere on your machine. Desktop will work.

Open STARS.Applications.VETS.Plugins.VTS.Interface.dll.config, edit the following values: 

	key="SharedFolder", value = folder path of the SharedFolder on your machine
	key="TempFolder", value = some folder on your machine to be used for creating temp files

Refer to SharedFolder/Config/readMe.txt for more information on how the VTS-VETS tool can be custom configured.