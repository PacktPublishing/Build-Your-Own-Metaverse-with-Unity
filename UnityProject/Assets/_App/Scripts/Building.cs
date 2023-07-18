using Firebase.Firestore;

[FirestoreData]
public class Building
{
	[FirestoreProperty]
	public string Id { get; set; }

	[FirestoreProperty]
	public string Prefab { get; set; }

	[FirestoreProperty]
	public string WorldId { get; set; }

	[FirestoreProperty]
	public float PosX { get; set; }

	[FirestoreProperty]
	public float PosY { get; set; }

	[FirestoreProperty]
	public float PosZ { get; set; }

	[FirestoreProperty]
	public string OwnerUserId { get; set; }

	public Building()
	{

	}
}