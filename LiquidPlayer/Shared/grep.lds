task "grep"

function Worker(Stream si, Stream so, string pattern)
{
	var r = new TextReader(si)
	var w = new TextWriter(so)

	until (r.EndOfStream())
	{
		var input = r.ReadLine()

		if (RegularExpression.IsMatch(input, pattern))
		{
			w.WriteLine(input)
		}
	}
}

main
{
	var commandLine = GetCommandLine()

	var count = commandLine.GetArgumentCount()

	if (count == 1)
	{
		Worker(StandardInput(), StandardOutput(), commandLine.GetArgument(0))
	}
	else if (count > 1)
	{
		var pattern = commandLine.GetArgument(0)

		for (var index = 1 to count - 1)
		{
			var argument = commandLine.GetArgument(index)

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
					Worker(fileStream, StandardOutput(), pattern)
					fileStream.Close()
				}
			}
		}
	}
	else
	{
		ErrorOut(GetTaskName() + ": Valid pattern expected")
	}
}
