task "head"

function Worker(Stream si, Stream so, int n)
{
    var r = new TextReader(si)
    var w = new TextWriter(so)

    for (var count = 1 to n)
    {
        if (r.EndOfStream())
        {
            break
        }

        w.WriteLine(r.ReadLine())
    }
}

main
{
    var commandLine = GetCommandLine()

    var n = commandLine.GetSwitch('n')

    if (n == -1)
    {
        n = 5
    }

    var count = commandLine.GetArgumentCount()

    if (count == 0)
    {
        Worker(StandardInput(), StandardOutput(), n)
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
                    Worker(fileStream, StandardOutput(), n)
                    fileStream.Close()
                }
            }
        }
    }
}
