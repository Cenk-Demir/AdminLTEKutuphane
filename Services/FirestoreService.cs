using Google.Cloud.Firestore;
using AdminLTEKutuphane.Models;
using System.Linq;
using System.Threading.Tasks;

namespace AdminLTEKutuphane.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreService(IConfiguration configuration)
        {
            string path = "/home/user/Belgeler/vs code proje/AdminLTEKutuphane/fir-kutuphane-lte-firebase-adminsdk-fbsvc-c28f20b2a2.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            string projectId = "fir-kutuphane-lte";
            _firestoreDb = FirestoreDb.Create(projectId);
        }




        #region Books
        // Kitapları al
        public async Task<List<Book>> GetBooksAsync()
        {
            var snapshot = await _firestoreDb.Collection("books").GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<Book>()).ToList();
        }

        // Kitap ekle
        public async Task AddBookAsync(Book book)
        {
            await _firestoreDb.Collection("books").AddAsync(book);
        }

        // Kitap ID'ye göre al
        public async Task<Book> GetBookByIdAsync(string bookId)
        {
            var docRef = _firestoreDb.Collection("books").Document(bookId);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<Book>() : null;
        }

        // Kitap güncelle
        public async Task UpdateBookAsync(string bookId, Book book)
        {
            var docRef = _firestoreDb.Collection("books").Document(bookId);
            await docRef.SetAsync(book, SetOptions.MergeAll);
        }

        // Kitap sil
        public async Task DeleteBookAsync(string bookId)
        {
            var docRef = _firestoreDb.Collection("books").Document(bookId);
            await docRef.DeleteAsync();
        }
        #endregion

        #region Users
        // Kullanıcıları al
        public async Task<List<User>> GetUsersAsync()
        {
            var snapshot = await _firestoreDb.Collection("users").GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<User>()).ToList();
        }

        // Kullanıcı ekle
        public async Task AddUserAsync(User user)
        {
            await _firestoreDb.Collection("users").AddAsync(user);
        }

        // Kullanıcı ID'ye göre al
        public async Task<User> GetUserByIdAsync(string userId)
        {
            var docRef = _firestoreDb.Collection("users").Document(userId);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<User>() : null;
        }

        // Kullanıcı güncelle
        public async Task UpdateUserAsync(string userId, User user)
        {
            var docRef = _firestoreDb.Collection("users").Document(userId);
            await docRef.SetAsync(user, SetOptions.MergeAll);
        }

        // Kullanıcı sil
        public async Task DeleteUserAsync(string userId)
        {
            var docRef = _firestoreDb.Collection("users").Document(userId);
            await docRef.DeleteAsync();
        }
        #endregion

        #region Borrowed Books
        // Ödünç alınan kitapları al
        public async Task<List<BorrowedBook>> GetBorrowedBooksAsync()
        {
            var snapshot = await _firestoreDb.Collection("borrowedBooks").GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<BorrowedBook>()).ToList();
        }

        // Ödünç alınan kitabı al
        public async Task<BorrowedBook> GetBorrowedBookByIdAsync(string borrowId)
        {
            var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowId);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<BorrowedBook>() : null;
        }

        // Ödünç kitap işlemi ekle
        public async Task AddBorrowTransactionAsync(BorrowedBook borrowBook)
        {
            await _firestoreDb.Collection("borrowedBooks").AddAsync(borrowBook);
        }

        // Ödünç kitap işlemini güncelle
        public async Task UpdateBorrowTransactionAsync(string borrowId, BorrowedBook borrowBook)
        {
            var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowId);
            await docRef.SetAsync(borrowBook, SetOptions.MergeAll);
        }

        // Ödünç kitap işlemini sil
        public async Task DeleteBorrowTransactionAsync(string borrowId)
        {
            var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowId);
            await docRef.DeleteAsync();
        }
        #endregion

        #region Counts and Queries
        // Kitap sayısını al
        public async Task<int> GetBookCountAsync()
        {
            var snapshot = await _firestoreDb.Collection("books").GetSnapshotAsync();
            return snapshot.Count;
        }

        // Kullanıcı sayısını al
        public async Task<int> GetUserCountAsync()
        {
            var snapshot = await _firestoreDb.Collection("users").GetSnapshotAsync();
            return snapshot.Count;
        }

        // Ödünç kitap sayısını al
        public async Task<int> GetBorrowedBookCountAsync()
        {
            var snapshot = await _firestoreDb.Collection("borrowedBooks").GetSnapshotAsync();
            return snapshot.Count;
        }

        // Aktif ödünç işlemlerinin sayısını al
        public async Task<int> GetActiveTransactionCountAsync()
        {
            var snapshot = await _firestoreDb.Collection("borrowedBooks")
                .WhereEqualTo("IsReturned", false)
                .GetSnapshotAsync();
            return snapshot.Count;
        }

        // Bir kullanıcının aktif ödünç kitaplarını al
        public async Task<List<BorrowedBook>> GetActiveBorrowedBooksByUserAsync(string userId)
        {
            var snapshot = await _firestoreDb.Collection("borrowedBooks")
                .WhereEqualTo("UserId", userId)
                .WhereEqualTo("IsReturned", false)
                .GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<BorrowedBook>()).ToList();
        }
        #endregion

        #region Existence Checks
        // Kitap var mı?
        public async Task<bool> BookExistsAsync(string bookId)
        {
            var docRef = _firestoreDb.Collection("books").Document(bookId);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists;
        }

        // Kullanıcı var mı?
        public async Task<bool> UserExistsAsync(string userId)
        {
            var docRef = _firestoreDb.Collection("users").Document(userId);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists;
        }

        // Ödünç kitap var mı?
        public async Task<bool> BorrowedBookExistsAsync(string borrowId)
        {
            var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowId);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists;
        }
        #endregion
    }
}
