using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace AdminLTEKutuphane.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreService()
        {
            // Firebase Admin SDK'sı ile bağlantı kuruyoruz.
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("~/Belgeler/vs code proje/AdminLTEKutuphane/fir-kutuphane-lte-firebase-adminsdk-fbsvc-c28f20b2a2.json")  // JSON dosyanızın yolunu buraya yazın
            });

            // Firestore ile bağlantı
            _firestoreDb = FirestoreDb.Create("fir-kutuphane-lte");  // Firebase projenizin ID'si
        }

        // Firestore'a veri eklemek için metot
        public async Task AddDocumentAsync(string collection, string documentId, Dictionary<string, object> data)
        {
            var docRef = _firestoreDb.Collection(collection).Document(documentId);
            await docRef.SetAsync(data);
        }

        // Firestore'dan veri almak için metot
        public async Task<DocumentSnapshot> GetDocumentAsync(string collection, string documentId)
        {
            var docRef = _firestoreDb.Collection(collection).Document(documentId);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot;
        }
    }
}
