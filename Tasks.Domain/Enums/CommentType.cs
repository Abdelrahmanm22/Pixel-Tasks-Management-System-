namespace Tasks.Domain.Enums
{
    /// <summary>
    /// Defines the content type of a task comment.
    /// Each comment is exactly one type — text, image, or a file attachment.
    /// </summary>
    public enum CommentType
    {
        Text  = 1,
        Image = 2,
        File  = 3
    }
}
