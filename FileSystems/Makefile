SOURCES=*/*.cs */*/*.cs
all: KFA.FileSystems.Lite.dll KFA.FileSystems.dll
KFA.FileSystems.Lite.dll: ${SOURCES}
	mcs -d:MONO -d:KFS_LEAN_AND_MEAN -o KFA.FileSystems.Lite.dll -t:library -r:System.Drawing.dll -r:System.Management.dll -r:System.Data.dll -r:Mono.Posix ${SOURCES}
KFA.FileSystems.dll: ${SOURCES}
	mcs -d:MONO -o KFA.FileSystems.dll -t:library -r:Ionic.Zip.dll -r:System.Drawing.dll -r:System.Management.dll -r:System.Data.dll -r:Mono.Posix ${SOURCES}
clean:
	rm KFA.FileSystems.dll KFA.FileSystems.Lite.dll
