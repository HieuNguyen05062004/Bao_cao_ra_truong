namespace Client.Extensions;

/// <summary>
/// Extension methods cho session
/// </summary>
public static class SessionExtensions
{
    public static bool IsReaderLoggedIn(this ISession session)
    {
        return !string.IsNullOrEmpty(session.GetString("ReaderId"));
    }

    public static string? GetReaderId(this ISession session)
    {
        return session.GetString("ReaderId");
    }

    public static string? GetReaderName(this ISession session)
    {
        return session.GetString("ReaderName");
    }

    public static void SetReaderSession(this ISession session, 
        string readerId, string readerName, string? email = null, string? avatar = null)
    {
        session.SetString("ReaderId", readerId);
        session.SetString("ReaderName", readerName);
        if (!string.IsNullOrEmpty(email))
            session.SetString("ReaderEmail", email);
        if (!string.IsNullOrEmpty(avatar))
            session.SetString("ReaderAvatar", avatar);
    }
}
