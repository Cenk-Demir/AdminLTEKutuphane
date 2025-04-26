using Google.Cloud.Firestore;
using AdminLTEKutuphane.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace AdminLTEKutuphane.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly IConfiguration _configuration;

        public FirestoreService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            string? path = _configuration["Firebase:ServiceAccountKeyPath"];
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Firebase:ServiceAccountKeyPath yapılandırması bulunamadı.");

            string? projectId = _configuration["Firebase:ProjectId"];
            if (string.IsNullOrEmpty(projectId))
                throw new ArgumentException("Firebase:ProjectId yapılandırması bulunamadı.");

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            _firestoreDb = FirestoreDb.Create(projectId);
        }

        #region Books
        // Kitapları al
        public async Task<List<Book>> GetBooksAsync()
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("books").GetSnapshotAsync();
                var books = new List<Book>();
                foreach (var doc in snapshot.Documents)
                {
                    var book = new Book
                    {
                        Id = doc.Id,
                        Title = doc.GetValue<string>("title"),
                        Author = doc.GetValue<string>("author"),
                        ISBN = doc.GetValue<string>("isbn"),
                        Publisher = doc.GetValue<string>("publisher"),
                        PublicationYear = doc.GetValue<int>("publicationYear"),
                        PageCount = doc.GetValue<int>("pageCount"),
                        Description = doc.GetValue<string>("description"),
                        Category = doc.GetValue<string>("category"),
                        ShelfNumber = doc.GetValue<string>("shelfNumber"),
                        IsAvailable = doc.GetValue<bool>("isAvailable"),
                        CreatedAt = doc.GetValue<DateTime>("createdAt"),
                        UpdatedAt = doc.GetValue<DateTime?>("updatedAt")
                    };
                    books.Add(book);
                }
                return books;
            }
            catch (Exception ex)
            {
                throw new Exception("Kitaplar alınırken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Kitap ekle
        public async Task AddBookAsync(Book book)
        {
            try
            {
                book.CreatedAt = DateTime.UtcNow;
                var docRef = await _firestoreDb.Collection("books").AddAsync(new Dictionary<string, object>
                {
                    { "title", book.Title },
                    { "author", book.Author },
                    { "isbn", book.ISBN },
                    { "publisher", book.Publisher },
                    { "publicationYear", book.PublicationYear },
                    { "pageCount", book.PageCount },
                    { "description", book.Description },
                    { "category", book.Category },
                    { "shelfNumber", book.ShelfNumber },
                    { "isAvailable", book.IsAvailable },
                    { "createdAt", book.CreatedAt },
                    { "updatedAt", book.UpdatedAt }
                });
                book.Id = docRef.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Kitap eklenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Kitap ID'ye göre al
        public async Task<Book> GetBookByIdAsync(string bookId)
        {
            try
            {
                var docRef = _firestoreDb.Collection("books").Document(bookId);
                var snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    return new Book
                    {
                        Id = snapshot.Id,
                        Title = snapshot.GetValue<string>("title"),
                        Author = snapshot.GetValue<string>("author"),
                        ISBN = snapshot.GetValue<string>("isbn"),
                        Publisher = snapshot.GetValue<string>("publisher"),
                        PublicationYear = snapshot.GetValue<int>("publicationYear"),
                        PageCount = snapshot.GetValue<int>("pageCount"),
                        Description = snapshot.GetValue<string>("description"),
                        Category = snapshot.GetValue<string>("category"),
                        ShelfNumber = snapshot.GetValue<string>("shelfNumber"),
                        IsAvailable = snapshot.GetValue<bool>("isAvailable"),
                        CreatedAt = snapshot.GetValue<DateTime>("createdAt"),
                        UpdatedAt = snapshot.GetValue<DateTime?>("updatedAt")
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Kitap bilgileri alınırken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Kitap güncelle
        public async Task UpdateBookAsync(string bookId, Book book)
        {
            try
            {
                book.UpdatedAt = DateTime.UtcNow;
                var docRef = _firestoreDb.Collection("books").Document(bookId);
                await docRef.SetAsync(new Dictionary<string, object>
                {
                    { "title", book.Title },
                    { "author", book.Author },
                    { "isbn", book.ISBN },
                    { "publisher", book.Publisher },
                    { "publicationYear", book.PublicationYear },
                    { "pageCount", book.PageCount },
                    { "description", book.Description },
                    { "category", book.Category },
                    { "shelfNumber", book.ShelfNumber },
                    { "isAvailable", book.IsAvailable },
                    { "updatedAt", book.UpdatedAt }
                }, SetOptions.MergeAll);
            }
            catch (Exception ex)
            {
                throw new Exception("Kitap güncellenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Kitap sil
        public async Task DeleteBookAsync(string bookId)
        {
            try
            {
                var docRef = _firestoreDb.Collection("books").Document(bookId);
                await docRef.DeleteAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Kitap silinirken bir hata oluştu: " + ex.Message, ex);
            }
        }
        #endregion

        #region Users
        // Kullanıcıları al
        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                var users = new List<User>();
                var collection = _firestoreDb.Collection("users");
                var snapshot = await collection.GetSnapshotAsync();

                foreach (var document in snapshot.Documents)
                {
                    var userData = document.ToDictionary();
                    var user = new User
                    {
                        Id = document.Id,
                        FirstName = userData.GetValueOrDefault("firstname", "")?.ToString() ?? 
                                   userData.GetValueOrDefault("FirstName", "")?.ToString() ?? "",
                                   
                        LastName = userData.GetValueOrDefault("lastname", "")?.ToString() ?? 
                                  userData.GetValueOrDefault("LastName", "")?.ToString() ?? "",
                                  
                        Email = userData.GetValueOrDefault("email", "")?.ToString() ?? 
                                userData.GetValueOrDefault("Email", "")?.ToString() ?? "",
                                
                        Role = userData.GetValueOrDefault("role", "")?.ToString() ?? 
                               userData.GetValueOrDefault("Role", "")?.ToString() ?? "",
                               
                        Phone = userData.GetValueOrDefault("phone", "")?.ToString() ?? 
                                userData.GetValueOrDefault("Phone", "")?.ToString() ?? "",
                                
                        Address = userData.GetValueOrDefault("address", "")?.ToString() ?? 
                                 userData.GetValueOrDefault("Address", "")?.ToString() ?? "",
                                 
                        MembershipNumber = userData.GetValueOrDefault("membershipnumber", "")?.ToString() ?? 
                                          userData.GetValueOrDefault("MembershipNumber", "")?.ToString() ?? "",
                                          
                        MembershipStatus = userData.GetValueOrDefault("membershipstatus", "")?.ToString() ?? 
                                          userData.GetValueOrDefault("MembershipStatus", "")?.ToString() ?? "",
                    };

                    // Tarih alanları için özel kontrol
                    if (userData.TryGetValue("membershipstartdate", out var startDate) || 
                        userData.TryGetValue("MembershipStartDate", out startDate))
                    {
                        user.MembershipStartDate = (startDate as Timestamp?)?.ToDateTime() ?? DateTime.MinValue;
                    }

                    if (userData.TryGetValue("membershipenddate", out var endDate) || 
                        userData.TryGetValue("MembershipEndDate", out endDate))
                    {
                        user.MembershipEndDate = (endDate as Timestamp?)?.ToDateTime() ?? DateTime.MinValue;
                    }

                    if (userData.TryGetValue("createdat", out var createdAt) || 
                        userData.TryGetValue("CreatedAt", out createdAt))
                    {
                        user.CreatedAt = (createdAt as Timestamp?)?.ToDateTime() ?? DateTime.MinValue;
                    }

                    if (userData.TryGetValue("updatedat", out var updatedAt) || 
                        userData.TryGetValue("UpdatedAt", out updatedAt))
                    {
                        user.UpdatedAt = (updatedAt as Timestamp?)?.ToDateTime() ?? DateTime.MinValue;
                    }

                    users.Add(user);
                    Console.WriteLine($"Retrieved user: {user.FirstName} {user.LastName} ({user.Email})");
                }

                Console.WriteLine($"Retrieved {users.Count} users");
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllUsersAsync hata: {ex.Message}");
                throw new Exception("Kullanıcılar getirilirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Kullanıcı ekle
        public async Task AddUserAsync(User user)
        {
            try
            {
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                user.MembershipStartDate = DateTime.UtcNow;
                user.MembershipStatus = "Active";
                
                var docRef = await _firestoreDb.Collection("users").AddAsync(new Dictionary<string, object>
                {
                    { "firstname", user.FirstName },
                    { "lastname", user.LastName },
                    { "email", user.Email },
                    { "password", user.Password },
                    { "role", user.Role },
                    { "phone", user.Phone ?? string.Empty },
                    { "address", user.Address ?? string.Empty },
                    { "membershipnumber", user.MembershipNumber ?? string.Empty },
                    { "membershipstatus", user.MembershipStatus },
                    { "membershipstartdate", user.MembershipStartDate },
                    { "membershipenddate", user.MembershipEndDate },
                    { "createdat", user.CreatedAt },
                    { "updatedat", user.UpdatedAt }
                });
                user.Id = docRef.Id;
                Console.WriteLine($"User added: {user.FirstName} {user.LastName} ({user.Email})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddUserAsync hata: {ex.Message}");
                throw new Exception("Kullanıcı eklenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Kullanıcı ID'ye göre al
        public async Task<User> GetUserAsync(string userId)
        {
            try
            {
                var docRef = _firestoreDb.Collection("users").Document(userId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    throw new Exception($"ID'si {userId} olan kullanıcı bulunamadı.");
                }

                var userData = snapshot.ToDictionary();
                var user = new User
                {
                    Id = snapshot.Id,
                    FirstName = userData.GetValueOrDefault("firstname", "").ToString(),
                    LastName = userData.GetValueOrDefault("lastname", "").ToString(),
                    Email = userData.GetValueOrDefault("email", "").ToString(),
                    Role = userData.GetValueOrDefault("role", "").ToString(),
                    Phone = userData.GetValueOrDefault("phone", "").ToString(),
                    Address = userData.GetValueOrDefault("address", "").ToString(),
                    MembershipNumber = userData.GetValueOrDefault("membershipnumber", "").ToString(),
                    MembershipStatus = userData.GetValueOrDefault("membershipstatus", "").ToString(),
                    MembershipStartDate = (userData.GetValueOrDefault("membershipstartdate") as Timestamp?)?.ToDateTime() ?? DateTime.MinValue,
                    MembershipEndDate = (userData.GetValueOrDefault("membershipenddate") as Timestamp?)?.ToDateTime() ?? DateTime.MinValue,
                    CreatedAt = (userData.GetValueOrDefault("createdat") as Timestamp?)?.ToDateTime() ?? DateTime.MinValue,
                    UpdatedAt = (userData.GetValueOrDefault("updatedat") as Timestamp?)?.ToDateTime() ?? DateTime.MinValue
                };

                Console.WriteLine($"User retrieved: {user.FirstName} {user.LastName} ({user.Email})");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserAsync hata: {ex.Message}");
                throw new Exception("Kullanıcı getirilirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Kullanıcı güncelle
        public async Task UpdateUserAsync(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.UtcNow;
                var docRef = _firestoreDb.Collection("users").Document(user.Id);
                var userDoc = await docRef.GetSnapshotAsync();

                if (!userDoc.Exists)
                {
                    throw new Exception($"ID'si {user.Id} olan kullanıcı bulunamadı.");
                }

                await docRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "firstname", user.FirstName },
                    { "lastname", user.LastName },
                    { "email", user.Email },
                    { "role", user.Role },
                    { "phone", user.Phone ?? string.Empty },
                    { "address", user.Address ?? string.Empty },
                    { "membershipnumber", user.MembershipNumber ?? string.Empty },
                    { "membershipstatus", user.MembershipStatus },
                    { "membershipstartdate", user.MembershipStartDate },
                    { "membershipenddate", user.MembershipEndDate },
                    { "updatedat", user.UpdatedAt }
                });
                Console.WriteLine($"User updated: {user.FirstName} {user.LastName} ({user.Email})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateUserAsync hata: {ex.Message}");
                throw new Exception("Kullanıcı güncellenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Kullanıcı sil
        public async Task DeleteUserAsync(string userId)
        {
            try
            {
                var docRef = _firestoreDb.Collection("users").Document(userId);
                await docRef.DeleteAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcı silinirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("users")
                    .WhereEqualTo("email", email)
                    .GetSnapshotAsync();
                return snapshot.Documents.Any();
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcı kontrolü yapılırken bir hata oluştu: " + ex.Message, ex);
            }
        }

        public async Task<bool> UserExistsByIdAsync(string userId)
        {
            try
            {
                var docRef = _firestoreDb.Collection("users").Document(userId);
                var snapshot = await docRef.GetSnapshotAsync();
                return snapshot.Exists;
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcı kontrolü yapılırken bir hata oluştu: " + ex.Message, ex);
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                var collection = _firestoreDb.Collection("users");
                var query = collection.WhereEqualTo("email", email);
                var snapshot = await query.GetSnapshotAsync();

                if (!snapshot.Any())
                {
                    throw new Exception($"'{email}' e-posta adresine sahip kullanıcı bulunamadı.");
                }

                var document = snapshot.FirstOrDefault();
                var userData = document.ToDictionary();

                var user = new User
                {
                    Id = document.Id,
                    FirstName = userData.GetValueOrDefault("firstname", "").ToString(),
                    LastName = userData.GetValueOrDefault("lastname", "").ToString(),
                    Email = userData.GetValueOrDefault("email", "").ToString(),
                    Role = userData.GetValueOrDefault("role", "").ToString(),
                    Phone = userData.GetValueOrDefault("phone", "").ToString(),
                    Address = userData.GetValueOrDefault("address", "").ToString(),
                    MembershipNumber = userData.GetValueOrDefault("membershipnumber", "").ToString(),
                    MembershipStatus = userData.GetValueOrDefault("membershipstatus", "").ToString(),
                    MembershipStartDate = (userData.GetValueOrDefault("membershipstartdate") as Timestamp?)?.ToDateTime() ?? DateTime.MinValue,
                    MembershipEndDate = (userData.GetValueOrDefault("membershipenddate") as Timestamp?)?.ToDateTime() ?? DateTime.MinValue,
                    CreatedAt = (userData.GetValueOrDefault("createdat") as Timestamp?)?.ToDateTime() ?? DateTime.MinValue,
                    UpdatedAt = (userData.GetValueOrDefault("updatedat") as Timestamp?)?.ToDateTime() ?? DateTime.MinValue
                };

                Console.WriteLine($"Retrieved user with email: {email}");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserByEmailAsync hata: {ex.Message}");
                throw new Exception("E-posta ile kullanıcı aranırken bir hata oluştu: " + ex.Message, ex);
            }
        }
        #endregion

        #region Borrowed Books
        // Ödünç alınan kitapları al
        public async Task<List<BorrowedBook>> GetBorrowedBooksAsync()
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("borrowedBooks").GetSnapshotAsync();
                var borrowedBooks = new List<BorrowedBook>();
                foreach (var doc in snapshot.Documents)
                {
                    var borrowedBook = new BorrowedBook
                    {
                        Id = doc.Id,
                        BookId = doc.GetValue<string>("bookId") ?? string.Empty,
                        UserId = doc.GetValue<string>("userId") ?? string.Empty,
                        BorrowDate = doc.GetValue<DateTime>("borrowDate"),
                        DueDate = doc.GetValue<DateTime>("dueDate"),
                        ReturnDate = doc.GetValue<DateTime?>("returnDate"),
                        IsReturned = doc.GetValue<bool>("isReturned"),
                        Notes = doc.GetValue<string>("notes") ?? string.Empty,
                        CreatedAt = doc.GetValue<DateTime>("CreatedAt"),
                        UpdatedAt = doc.GetValue<DateTime?>("UpdatedAt")
                    };
                    borrowedBooks.Add(borrowedBook);
                }
                return borrowedBooks;
            }
            catch (Exception ex)
            {
                throw new Exception("Ödünç alınan kitaplar alınırken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Ödünç alınan kitabı al
        public async Task<BorrowedBook> GetBorrowedBookByIdAsync(string borrowedBookId)
        {
            try
            {
                var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowedBookId);
                var snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    return new BorrowedBook
                    {
                        Id = snapshot.Id,
                        BookId = snapshot.GetValue<string>("bookId"),
                        UserId = snapshot.GetValue<string>("userId"),
                        BorrowDate = snapshot.GetValue<DateTime>("borrowDate"),
                        DueDate = snapshot.GetValue<DateTime>("dueDate"),
                        ReturnDate = snapshot.GetValue<DateTime?>("returnDate"),
                        IsReturned = snapshot.GetValue<bool>("isReturned"),
                        Notes = snapshot.GetValue<string>("notes"),
                        CreatedAt = snapshot.GetValue<DateTime>("CreatedAt"),
                        UpdatedAt = snapshot.GetValue<DateTime?>("UpdatedAt")
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Ödünç kitap bilgileri alınırken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Ödünç kitap işlemi ekle
        public async Task AddBorrowTransactionAsync(BorrowedBook borrowBook)
        {
            try
            {
                borrowBook.CreatedAt = DateTime.UtcNow;
                var docRef = await _firestoreDb.Collection("borrowedBooks").AddAsync(new Dictionary<string, object>
                {
                    { "bookId", borrowBook.BookId },
                    { "userId", borrowBook.UserId },
                    { "borrowDate", borrowBook.BorrowDate },
                    { "dueDate", borrowBook.DueDate },
                    { "returnDate", borrowBook.ReturnDate },
                    { "isReturned", borrowBook.IsReturned },
                    { "notes", borrowBook.Notes ?? string.Empty },
                    { "CreatedAt", borrowBook.CreatedAt },
                    { "UpdatedAt", borrowBook.UpdatedAt }
                });
                borrowBook.Id = docRef.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Ödünç kitap işlemi eklenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Ödünç kitap işlemini güncelle
        public async Task UpdateBorrowTransactionAsync(string borrowId, BorrowedBook borrowBook)
        {
            try
            {
                borrowBook.UpdatedAt = DateTime.UtcNow;
                var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowId);
                await docRef.SetAsync(new Dictionary<string, object>
                {
                    { "bookId", borrowBook.BookId },
                    { "userId", borrowBook.UserId },
                    { "borrowDate", borrowBook.BorrowDate },
                    { "dueDate", borrowBook.DueDate },
                    { "returnDate", borrowBook.ReturnDate },
                    { "isReturned", borrowBook.IsReturned },
                    { "notes", borrowBook.Notes },
                    { "updatedAt", borrowBook.UpdatedAt }
                }, SetOptions.MergeAll);
            }
            catch (Exception ex)
            {
                throw new Exception("Ödünç kitap işlemi güncellenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Ödünç kitap işlemini sil
        public async Task DeleteBorrowTransactionAsync(string borrowId)
        {
            var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowId);
            await docRef.DeleteAsync();
        }

        // Bir kullanıcının aktif ödünç kitaplarını al
        public async Task<List<BorrowedBook>> GetActiveBorrowedBooksByUserAsync(string userId)
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("borrowedBooks")
                    .WhereEqualTo("UserId", userId)
                    .WhereEqualTo("IsReturned", false)
                    .GetSnapshotAsync();
                var borrowedBooks = new List<BorrowedBook>();
                foreach (var doc in snapshot.Documents)
                {
                    var borrowedBook = doc.ConvertTo<BorrowedBook>();
                    borrowedBook.Id = doc.Id;
                    borrowedBooks.Add(borrowedBook);
                }
                return borrowedBooks;
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcının aktif ödünç kitapları alınırken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Ödünç kitap ekle
        public async Task AddBorrowedBookAsync(BorrowedBook borrowBook)
        {
            try
            {
                borrowBook.CreatedAt = DateTime.UtcNow;
                var docRef = await _firestoreDb.Collection("borrowedBooks").AddAsync(borrowBook);
                borrowBook.Id = docRef.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Ödünç kitap eklenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        // Ödünç kitap güncelle
        public async Task UpdateBorrowedBookAsync(string borrowId, BorrowedBook borrowBook)
        {
            try
            {
                borrowBook.UpdatedAt = DateTime.UtcNow;
                var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowId);
                await docRef.SetAsync(borrowBook, SetOptions.MergeAll);
            }
            catch (Exception ex)
            {
                throw new Exception("Ödünç kitap güncellenirken bir hata oluştu: " + ex.Message, ex);
            }
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

        // Dashboard için sayaç metodları
        public async Task<int> GetBookCount()
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("books").GetSnapshotAsync();
                return snapshot.Count;
            }
            catch (Exception ex)
            {
                throw new Exception("Kitap sayısı alınırken hata oluştu", ex);
            }
        }

        public async Task<int> GetUserCount()
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("users").GetSnapshotAsync();
                return snapshot.Count;
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcı sayısı alınırken hata oluştu", ex);
            }
        }

        public async Task<int> GetBorrowedBookCount()
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("borrowedbooks")
                    .WhereEqualTo("isreturned", false)
                    .GetSnapshotAsync();
                return snapshot.Count;
            }
            catch (Exception ex)
            {
                throw new Exception("Ödünç alınan kitap sayısı alınırken hata oluştu", ex);
            }
        }

        public async Task<int> GetActiveTransactionCount()
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("transactions")
                    .WhereEqualTo("transactiontype", "borrow")
                    .WhereEqualTo("returndate", null)
                    .GetSnapshotAsync();
                return snapshot.Count;
            }
            catch (Exception ex)
            {
                throw new Exception("Aktif işlem sayısı alınırken hata oluştu", ex);
            }
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
