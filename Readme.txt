Der ProjectImporter ist ein kleines Tool, das dabei hilft Abgaben von Studenten aus dem elearning in Visual Studio zu importieren. Dabei werden die Ordner der Abgaben umbenannt und in jedem dieser Ordner ein eigenes Visual Studio Projekt generiert, welche dann in der eigenen Solution import werden können (entweder automatisch oder manuell).
Wer Fehler findet, darf sie behalten.

Parameter:

Project Prefix -> 					Das, mit dem alle Projekte geprefixed werden. Die Projektnamen setzen sich aus dem Prefix und dem Nachnamen des Studenten zusammen
									z.B.: Partikelsystem Abgabe von Max Mustermann bekommt ein Projekt -> PartikelsystemMaxMustermann; Margit Musterfrau -> PartikelsystemMargitMusterfrau

Assignment Directory -> 			Das Verzeichnis, das die Abgaben der Studenten enthält. (Der Ordner, wohin das Zip file vom elearning entpackt wurde)

Additional Dependencies -> 			Zusätzliche Abhängigkeiten, die man in Visual Studio unter Projekt->Eigenschaften->Linker->Eingabe angeben kann. Z.B.: der Pfad zu den ASTU. Ich weiß nicht, ob das in Visual Studio Code notwendig ist.
									(In meinem Fall: "G:\\Programmordner\\OneDrive\\OneDrive - students.fh-hagenberg.at\\FH\\Master 3. Semester\\AST Tutorium\\ASTU\\out\\build\\Debug\\astu.lib";"G:\\Programmordner\\OneDrive\\OneDrive - students.fh-hagenberg.at\\FH\\Master 3. Semester\\AST Tutorium\\SDL2-2.0.14\\lib\\x64\\SDL2.lib";%(AdditionalDependencies))

Additional Includes -> 				Zusätzliche Includes, die man in Visual Studio unter Projekt->Eigenschaften->C/C++->Allgemein angeben kann. Z.B.: der Pfad zu den ASTU. Ich weiß nicht, ob das in Visual Studio Code notwendig ist.
									(In meinem Fall: G:\\Programmordner\\OneDrive\\OneDrive - students.fh-hagenberg.at\\FH\\Master 3. Semester\\AST Tutorium\\ASTU\\include;%(AdditionalIncludeDirectories))

Auto search source files -> 		Versucht automatisch Quelldateien zu finden und in das Projekt einzubinden.
									Wenn für jeden Schwierigkeitsgrad ein Unterordner vorhanden ist, funktioniert das nicht, weil alle source files ins gleiche Projekt geschmissen werden!

Auto import to master solution ->	importiert die generierten Projekte automatisch in die angegebene Master solution. Bitte vorher ein backup von der .sln Datei anlegen!

Master solution file ->				Der Pfad zum master solution file. Nur relevant, wenn auto import ausgewählt ist.