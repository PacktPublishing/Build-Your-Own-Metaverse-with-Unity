using Firebase.Firestore; 

[FirestoreData] 
public class World 
{ 
    [FirestoreProperty] 
    public string Description { get; set; } 

    [FirestoreProperty] 
    public string Name { get; set; } 

    [FirestoreProperty] 
    public string Scene { get; set; } 
 
    public World() 
    { 

    } 
}  