task "tail"

function Worker(Stream si, Stream so, int n)
{
    var r = new TextReader(si)
    var w = new TextWriter(so)

    var count = 0
    var lines = new Array<string>(n)

    until (r.EndOfStream() || count >= n)
    {
        lines[count] = r.ReadLine()
        count = count + 1
    }

    until (r.EndOfStream())
    {
        lines.Delete(0)
        lines[n - 1] = r.ReadLine()
    }

    for (var Inx = 0 to count - 1)
    {
        w.WriteLine(lines[Inx])
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
