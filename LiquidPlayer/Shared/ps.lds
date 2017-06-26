task "ps"

main
{
    var so = StandardOutput()
    
    var w = new TextWriter(so)
 
    var parentTask = GetParentTask()
    
    var taskList = GetTaskList(parentTask)
    
    foreach (var line in taskList)
    {
        w.WriteLine(line)
    }
}
