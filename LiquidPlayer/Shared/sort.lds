task "sort"

function Worker(Stream si, Stream so, boolean reverse)
{
    var r = new TextReader(si)
    var w = new TextWriter(so)

    var lines = new List<string>()

    until (r.EndOfStream())
    {
        lines.Add(r.ReadLine())
    }

    lines.Sort()

    if (reverse)
    {
        lines.Reverse()
    }

    foreach (var line in lines)
    {
        w.WriteLine(line)
    }
}

main
{
    var commandLine = GetCommandLine()

    var reverse = false

    var r = commandLine.GetSwitch('r')

    if (r == 1)
    {
        reverse = true
    }

    var count = commandLine.GetArgumentCount()

    if (count == 0)
    {
        Worker(StandardInput(), StandardOutput(), reverse)
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
                    Worker(fileStream, StandardOutput(), reverse)
                    fileStream.Close()
                }
            }
        }
    }
}
