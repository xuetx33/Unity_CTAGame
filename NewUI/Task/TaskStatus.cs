// TaskStatus.cs
public enum TaskStatus
{
    NotAccepted = 0,    // 未接受
    InProgress = 1,     // 已接受未完成
    Completable = 2,    // 已完成可提交
    Completed = 3       // 已完成已提交
}