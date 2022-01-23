SOURCES=*.cs */*.cs
LIBS=../FileSystems/KFA.FileSystems.dll
RESOURCES=Explorers/FileExplorer.resources ProgressPopup.resources
.SUFFIXES:
.SUFFIXES: .resx .resources
.resx.resources:
	resgen $<
KFA.GuiComponents.dll: ${SOURCES} ${LIBS|} ${RESOURCES}
	make -C ../FileSystems
	mcs -t:library -o KFA.GuiComponents.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll -r:System.Data.dll -r:../FileSystems/KFA.FileSystems.dll $(addprefix -resource:,${RESOURCES}) ${SOURCES}
clean:
	rm KFA.GuiComponents.dll
