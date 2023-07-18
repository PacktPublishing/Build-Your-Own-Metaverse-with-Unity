using Firebase.Firestore;

[FirestoreData]
public class UserData
{
	[FirestoreProperty]
	public string Uid { get; set; }

	[FirestoreProperty]
	public string Nickname { get; set; }

	[FirestoreProperty]
	public string Username { get; set; }

	public UserData()
	{

	}
}