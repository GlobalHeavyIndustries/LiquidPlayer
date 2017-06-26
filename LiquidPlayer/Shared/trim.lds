task "trim"

function Worker(Stream si, Stream so)
{
	var r = new TextReader(si)
	var w = new TextWriter(so)

	until (r.EndOfStream())
	{
		w.WriteLine(string.Trim(r.ReadLine()))
	}
}

main
{
	var commandLine = GetCommandLine()

	var count = commandLine.GetArgumentCount()

	if (count == 0)
	{
		Worker(StandardInput(), StandardOutput())
	}
	else
	{
		foreach (var argument in commandLine.GetArguments())
		{
			var fileList = FileSystem.GetFileList(argument)

			if (fileList.GetCount() == 0)
			{
				ErrorOut(GetTaskName() + ": File not found")
			}
			else
			{
				foreach (var fileName in fileList)
				{
					var fileStream = new FileStream(fileName)

					fileStream.Open()
					Worker(fileStream, StandardOutput())
					fileStream.Close()
				}
			}
		}
	}
}
