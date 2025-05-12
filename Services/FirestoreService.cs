using Google.Cloud.Firestore;
using AdminLTEKutuphane.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Dynamic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        private Timestamp ConvertToTimestamp(DateTime dateTime)
        {
            return Timestamp.FromDateTime(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
        }

        private DateTime ConvertFromTimestamp(Timestamp timestamp)
        {
            return timestamp.ToDateTime();
        }

        // Timestamp'ı DateTime'a dönüştürme
    public DateTime ConvertTimestampToDateTime(Timestamp timestamp)
    {
        return timestamp.ToDateTime();
    }

    // Nullable Timestamp'ı DateTime'a dönüştürme
    public DateTime? ConvertNullableTimestampToDateTime(Timestamp? timestamp)
    {
        if (timestamp.HasValue)
        {
            return timestamp.Value.ToDateTime();
        }
        return null; // Null ise null döndürüyoruz
    }

        private Dictionary<string, object> ConvertToDictionary<T>(T entity)
        {
            var dictionary = new Dictionary<string, object>();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(entity);
                if (value != null)
                {
                    if (property.PropertyType == typeof(DateTime))
                    {
                        var dateTime = (DateTime)value;
                        dictionary[property.Name] = ConvertToTimestamp(dateTime);
                    }
                    else if (property.PropertyType == typeof(DateTime?))
                    {
                        var nullableDateTime = (DateTime?)value;
                        if (nullableDateTime.HasValue)
                        {
                            dictionary[property.Name] = ConvertToTimestamp(nullableDateTime.Value);
                        }
                    }
                    else
                    {
                        dictionary[property.Name] = value;
                    }
                }
                else if (property.PropertyType == typeof(string))
                {
                    dictionary[property.Name] = string.Empty;
                }
            }

            return dictionary;
        }

        private T ConvertFromDictionary<T>(Dictionary<string, object> dictionary) where T : new()
        {
            var entity = new T();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                if (dictionary.TryGetValue(property.Name, out var value))
                {
                    if (value is Timestamp timestamp)
                    {
                        if (property.PropertyType == typeof(DateTime))
                        {
                            property.SetValue(entity, ConvertFromTimestamp(timestamp));
                        }
                        else if (property.PropertyType == typeof(DateTime?))
                        {
                            property.SetValue(entity, ConvertFromTimestamp(timestamp));
                        }
                    }
                    else if (value != null)
                    {
                        try
                        {
                            if (property.PropertyType == typeof(string))
                            {
                                property.SetValue(entity, value.ToString() ?? string.Empty);
                            }
                            else
                            {
                                property.SetValue(entity, Convert.ChangeType(value, property.PropertyType));
                            }
                        }
                        catch
                        {
                            if (property.PropertyType == typeof(string))
                            {
                                property.SetValue(entity, string.Empty);
                            }
                            else
                            {
                                property.SetValue(entity, null);
                            }
                        }
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(entity, string.Empty);
                    }
                }
                else if (property.PropertyType == typeof(string))
                {
                    property.SetValue(entity, string.Empty);
                }
            }

            return entity;
        }

        public async Task<T?> GetByIdAsync<T>(string collection, string id) where T : new()
        {
            try
            {
                var docRef = _firestoreDb.Collection(collection).Document(id);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return default;
                }

                var dictionary = snapshot.ToDictionary();
                return ConvertFromDictionary<T>(dictionary);
            }
            catch (Exception)
            {
                return default;
            }
        }

        public async Task<T?> GetByEmailAsync<T>(string collection, string email) where T : new()
        {
            try
            {
                var query = _firestoreDb.Collection(collection)
                    .WhereEqualTo("Email", email)
                    .Limit(1);

                var snapshot = await query.GetSnapshotAsync();
                var document = snapshot.Documents.FirstOrDefault();

                if (document == null)
                {
                    return default;
                }

                var dictionary = document.ToDictionary();
                return ConvertFromDictionary<T>(dictionary);
            }
            catch (Exception)
            {
                return default;
            }
        }

        public async Task<List<T>> GetAllAsync<T>(string collection) where T : new()
        {
            try
            {
                var query = _firestoreDb.Collection(collection);
                var snapshot = await query.GetSnapshotAsync();

                return snapshot.Documents
                    .Select(doc => ConvertFromDictionary<T>(doc.ToDictionary()))
                    .ToList();
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }

        public async Task<string> AddAsync<T>(string collection, T entity)
        {
            var docRef = _firestoreDb.Collection(collection).Document();
            var dictionary = ConvertToDictionary(entity);
            await docRef.SetAsync(dictionary);
            return docRef.Id;
        }

        public async Task UpdateAsync<T>(string collection, string id, T entity)
        {
            var docRef = _firestoreDb.Collection(collection).Document(id);
            var dictionary = ConvertToDictionary(entity);
            await docRef.SetAsync(dictionary);
        }

        public async Task DeleteAsync(string collection, string id)
        {
            var docRef = _firestoreDb.Collection(collection).Document(id);
            await docRef.DeleteAsync();
        }

        public async Task<List<T>> QueryAsync<T>(string collection, string field, object value) where T : new()
        {
            try
            {
                var query = _firestoreDb.Collection(collection)
                    .WhereEqualTo(field, value);

                var snapshot = await query.GetSnapshotAsync();
                return snapshot.Documents
                    .Select(doc => ConvertFromDictionary<T>(doc.ToDictionary()))
                    .ToList();
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }

        #region Books
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
                        Title = doc.GetValue<string>("title") ?? string.Empty,
                        Author = doc.GetValue<string>("author") ?? string.Empty,
                        ISBN = doc.GetValue<string>("isbn") ?? string.Empty,
                        Publisher = doc.GetValue<string>("publisher") ?? string.Empty,
                        PublicationYear = doc.GetValue<int>("publicationYear"),
                        PageCount = doc.GetValue<int>("pageCount"),
                        Description = doc.GetValue<string>("description") ?? string.Empty,
                        Category = doc.GetValue<string>("category") ?? string.Empty,
                        ShelfNumber = doc.GetValue<string>("shelfNumber") ?? string.Empty,
                        IsAvailable = doc.GetValue<bool>("isAvailable"),
                        CreatedAt = doc.GetValue<Timestamp>("createdAt") != null ? doc.GetValue<Timestamp>("createdAt").ToDateTime() : DateTime.UtcNow,
                        UpdatedAt = doc.GetValue<Timestamp>("updatedAt") != null ? doc.GetValue<Timestamp>("updatedAt").ToDateTime() : DateTime.UtcNow,

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

        public async Task AddBookAsync(Book book)
        {
            try
            {
                book.CreatedAt = DateTime.UtcNow;
                var docRef = await _firestoreDb.Collection("books").AddAsync(new Dictionary<string, object>
                {
                    { "title", book.Title ?? string.Empty },
                    { "author", book.Author ?? string.Empty },
                    { "isbn", book.ISBN ?? string.Empty },
                    { "publisher", book.Publisher ?? string.Empty },
                    { "publicationYear", book.PublicationYear },
                    { "pageCount", book.PageCount },
                    { "description", book.Description ?? string.Empty },
                    { "category", book.Category ?? string.Empty },
                    { "shelfNumber", book.ShelfNumber ?? string.Empty },
                    { "isAvailable", book.IsAvailable },
                    { "createdAt", Timestamp.FromDateTime(DateTime.SpecifyKind(book.CreatedAt, DateTimeKind.Utc)) },
                    { "updatedAt", book.UpdatedAt.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(book.UpdatedAt.Value, DateTimeKind.Utc)) : null }
                });
                book.Id = docRef.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Kitap eklenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        public async Task<Book?> GetBookByIdAsync(string bookId)
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
                        Title = snapshot.GetValue<string>("title") ?? string.Empty,
                        Author = snapshot.GetValue<string>("author") ?? string.Empty,
                        ISBN = snapshot.GetValue<string>("isbn") ?? string.Empty,
                        Publisher = snapshot.GetValue<string>("publisher") ?? string.Empty,
                        PublicationYear = snapshot.GetValue<int>("publicationYear"),
                        PageCount = snapshot.GetValue<int>("pageCount"),
                        Description = snapshot.GetValue<string>("description") ?? string.Empty,
                        Category = snapshot.GetValue<string>("category") ?? string.Empty,
                        ShelfNumber = snapshot.GetValue<string>("shelfNumber") ?? string.Empty,
                        IsAvailable = snapshot.GetValue<bool>("isAvailable"),
                        CreatedAt = snapshot.GetValue<Timestamp>("createdAt") != null ? snapshot.GetValue<Timestamp>("createdAt").ToDateTime() : DateTime.UtcNow,
                        UpdatedAt = snapshot.GetValue<Timestamp>("updatedAt").ToDateTime()
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Kitap bilgileri alınırken bir hata oluştu: " + ex.Message, ex);
            }
        }

        public async Task UpdateBookAsync(string bookId, Book book)
        {
            try
            {
                book.UpdatedAt = DateTime.UtcNow;
                var docRef = _firestoreDb.Collection("books").Document(bookId);
                await docRef.SetAsync(new Dictionary<string, object>
                {
                    { "title", book.Title ?? string.Empty },
                    { "author", book.Author ?? string.Empty },
                    { "isbn", book.ISBN ?? string.Empty },
                    { "publisher", book.Publisher ?? string.Empty },
                    { "publicationYear", book.PublicationYear },
                    { "pageCount", book.PageCount },
                    { "description", book.Description ?? string.Empty },
                    { "category", book.Category ?? string.Empty },
                    { "shelfNumber", book.ShelfNumber ?? string.Empty },
                    { "isAvailable", book.IsAvailable },
                    { "updatedAt", Timestamp.FromDateTime(DateTime.SpecifyKind(book.UpdatedAt.Value, DateTimeKind.Utc)) }
                }, SetOptions.MergeAll);
            }
            catch (Exception ex)
            {
                throw new Exception("Kitap güncellenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

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
                        FirstName = GetStringValue(userData, "firstname", "FirstName"),
                        LastName = GetStringValue(userData, "lastname", "LastName"),
                        Email = GetStringValue(userData, "email", "Email"),
                        Role = GetStringValue(userData, "role", "Role"),
                        Phone = GetStringValue(userData, "phone", "Phone"),
                        Address = GetStringValue(userData, "address", "Address"),
                        MembershipNumber = GetStringValue(userData, "membershipnumber", "MembershipNumber"),
                        MembershipStatus = GetStringValue(userData, "membershipstatus", "MembershipStatus"),
                    };

                    var startDate = GetTimestampValue(userData, "membershipstartdate", "MembershipStartDate");
                    if (startDate.HasValue)
                    {
                        user.MembershipStartDate = startDate.Value;
                    }

                    var endDate = GetTimestampValue(userData, "membershipenddate", "MembershipEndDate");
                    if (endDate.HasValue)
                    {
                        user.MembershipEndDate = endDate;
                    }

                    var createdAt = GetTimestampValue(userData, "createdat", "CreatedAt");
                    if (createdAt.HasValue)
                    {
                        user.CreatedAt = createdAt.Value;
                    }

                    var updatedAt = GetTimestampValue(userData, "updatedat", "UpdatedAt");
                    if (updatedAt.HasValue)
                    {
                        user.UpdatedAt = updatedAt;
                    }

                    users.Add(user);
                }

                return users;
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcılar getirilirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        private string GetStringValue(Dictionary<string, object> data, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (data.TryGetValue(key, out var value) && value != null)
                {
                    return value.ToString() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        private DateTime? GetTimestampValue(Dictionary<string, object> data, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (data.TryGetValue(key, out var value) && value is Timestamp timestamp)
                {
                    return ConvertFromTimestamp(timestamp);
                }
            }
            return null;
        }

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
                    { "firstname", user.FirstName ?? string.Empty },
                    { "lastname", user.LastName ?? string.Empty },
                    { "email", user.Email?.ToLower() ?? string.Empty },
                    { "password", user.Password ?? string.Empty },
                    { "role", user.Role ?? "User" },
                    { "phone", user.Phone ?? string.Empty },
                    { "address", user.Address ?? string.Empty },
                    { "job", user.Job ?? string.Empty },
                    { "membershipnumber", user.MembershipNumber ?? string.Empty },
                    { "membershipstatus", user.MembershipStatus ?? "Active" },
                    { "membershipstartdate", ConvertToTimestamp(user.MembershipStartDate) },
                    { "membershipenddate", user.MembershipEndDate.HasValue ? ConvertToTimestamp(user.MembershipEndDate.Value) : null },
                    { "createdat", ConvertToTimestamp(user.CreatedAt) },
                    { "updatedat", ConvertToTimestamp(user.UpdatedAt ?? DateTime.UtcNow) }
                });
                user.Id = docRef.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcı eklenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        public async Task<User?> GetUserAsync(string userId)
        {
            try
            {
                var docRef = _firestoreDb.Collection("users").Document(userId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return null;
                }

                var userData = snapshot.ToDictionary();
                var user = new User
                {
                    Id = snapshot.Id,
                    FirstName = GetStringValue(userData, "firstname", "FirstName"),
                    LastName = GetStringValue(userData, "lastname", "LastName"),
                    Email = GetStringValue(userData, "email", "Email"),
                    Role = GetStringValue(userData, "role", "Role"),
                    Phone = GetStringValue(userData, "phone", "Phone"),
                    Address = GetStringValue(userData, "address", "Address"),
                    MembershipNumber = GetStringValue(userData, "membershipnumber", "MembershipNumber"),
                    MembershipStatus = GetStringValue(userData, "membershipstatus", "MembershipStatus"),
                };

                var startDate = GetTimestampValue(userData, "membershipstartdate", "MembershipStartDate");
                if (startDate.HasValue)
                {
                    user.MembershipStartDate = startDate.Value;
                }

                var endDate = GetTimestampValue(userData, "membershipenddate", "MembershipEndDate");
                if (endDate.HasValue)
                {
                    user.MembershipEndDate = endDate;
                }

                var createdAt = GetTimestampValue(userData, "createdat", "CreatedAt");
                if (createdAt.HasValue)
                {
                    user.CreatedAt = createdAt.Value;
                }

                var updatedAt = GetTimestampValue(userData, "updatedat", "UpdatedAt");
                if (updatedAt.HasValue)
                {
                    user.UpdatedAt = updatedAt;
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcı getirilirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        public async Task UpdateUserAsync(string userId, User user)
        {
            try
            {
                var docRef = _firestoreDb.Collection("users").Document(userId);
                var userDoc = await docRef.GetSnapshotAsync();

                if (!userDoc.Exists)
                {
                    throw new Exception($"ID'si {userId} olan kullanıcı bulunamadı.");
                }

                user.UpdatedAt = DateTime.UtcNow;
                
                var updateData = new Dictionary<string, object>
                {
                    { "firstname", user.FirstName ?? string.Empty },
                    { "lastname", user.LastName ?? string.Empty },
                    { "email", user.Email ?? string.Empty },
                    { "role", user.Role ?? string.Empty },
                    { "phone", user.Phone ?? string.Empty },
                    { "address", user.Address ?? string.Empty },
                    { "membershipnumber", user.MembershipNumber ?? string.Empty },
                    { "membershipstatus", user.MembershipStatus ?? "Active" },
                    { "updatedat", ConvertToTimestamp(user.UpdatedAt.Value) }
                };

                if (user.MembershipStartDate != DateTime.MinValue)
                {
                    updateData["membershipstartdate"] = ConvertToTimestamp(user.MembershipStartDate);
                }

                if (user.MembershipEndDate.HasValue)
                {
                    updateData["membershipenddate"] = ConvertToTimestamp(user.MembershipEndDate.Value);
                }

                if (!string.IsNullOrEmpty(user.Password))
                {
                    updateData["password"] = user.Password;
                }

                await docRef.SetAsync(updateData, SetOptions.MergeAll);
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcı güncellenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

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

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                var collection = _firestoreDb.Collection("users");
                var query = collection.WhereEqualTo("email", email.ToLower());
                var snapshot = await query.GetSnapshotAsync();

                if (!snapshot.Any())
                {
                    return null;
                }

                var document = snapshot.FirstOrDefault();
                if (document == null)
                {
                    return null;
                }

                var userData = document.ToDictionary();
                var user = new User
                {
                    Id = document.Id,
                    FirstName = GetStringValue(userData, "firstname", "FirstName"),
                    LastName = GetStringValue(userData, "lastname", "LastName"),
                    Email = GetStringValue(userData, "email", "Email"),
                    Role = GetStringValue(userData, "role", "Role"),
                    Phone = GetStringValue(userData, "phone", "Phone"),
                    Address = GetStringValue(userData, "address", "Address"),
                    MembershipNumber = GetStringValue(userData, "membershipnumber", "MembershipNumber"),
                    MembershipStatus = GetStringValue(userData, "membershipstatus", "MembershipStatus"),
                    Password = GetStringValue(userData, "password", "Password"),
                    Job = GetStringValue(userData, "job", "Job"),
                };

                var startDate = GetTimestampValue(userData, "membershipstartdate", "MembershipStartDate");
                if (startDate.HasValue)
                {
                    user.MembershipStartDate = startDate.Value;
                }

                var endDate = GetTimestampValue(userData, "membershipenddate", "MembershipEndDate");
                if (endDate.HasValue)
                {
                    user.MembershipEndDate = endDate;
                }

                var createdAt = GetTimestampValue(userData, "createdat", "CreatedAt");
                if (createdAt.HasValue)
                {
                    user.CreatedAt = createdAt.Value;
                }

                var updatedAt = GetTimestampValue(userData, "updatedat", "UpdatedAt");
                if (updatedAt.HasValue)
                {
                    user.UpdatedAt = updatedAt;
                }

                var lastLoginAt = GetTimestampValue(userData, "lastloginat", "LastLoginAt");
                if (lastLoginAt.HasValue)
                {
                    user.LastLoginAt = lastLoginAt;
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("E-posta ile kullanıcı aranırken bir hata oluştu: " + ex.Message, ex);
            }
        }
        #endregion

        #region Borrowed Books
        public async Task<List<BorrowedBook>> GetBorrowedBooksAsync()
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("borrowedBooks").GetSnapshotAsync();
                var borrowedBooks = new List<BorrowedBook>();
                
                foreach (var doc in snapshot.Documents)
                {
                    try
                    {
                        var data = doc.ToDictionary();
                        var borrowedBook = new BorrowedBook
                        {
                            Id = doc.Id,
                            BookId = data.GetValueOrDefault("bookId", "")?.ToString() ?? string.Empty,
                            UserId = data.GetValueOrDefault("userId", "")?.ToString() ?? string.Empty,
                            BorrowDate = (data.GetValueOrDefault("borrowDate") as Timestamp?)?.ToDateTime() ?? DateTime.UtcNow,
                            DueDate = (data.GetValueOrDefault("dueDate") as Timestamp?)?.ToDateTime() ?? DateTime.UtcNow,
                            ReturnDate = (data.GetValueOrDefault("returnDate") as Timestamp?)?.ToDateTime(),
                            IsReturned = data.GetValueOrDefault("isReturned", false) as bool? ?? false,
                            Notes = data.GetValueOrDefault("notes", "")?.ToString() ?? string.Empty,
                            CreatedAt = (data.GetValueOrDefault("createdAt") as Timestamp?)?.ToDateTime() ?? DateTime.UtcNow,
                            UpdatedAt = (data.GetValueOrDefault("updatedAt") as Timestamp?)?.ToDateTime()
                        };

                        if (!string.IsNullOrEmpty(borrowedBook.BookId))
                        {
                            try
                            {
                                borrowedBook.Book = await GetBookByIdAsync(borrowedBook.BookId);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Kitap bilgileri alınamadı (ID: {borrowedBook.BookId}): {ex.Message}");
                            }
                        }

                        if (!string.IsNullOrEmpty(borrowedBook.UserId))
                        {
                            try
                            {
                                borrowedBook.User = await GetUserAsync(borrowedBook.UserId);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Kullanıcı bilgileri alınamadı (ID: {borrowedBook.UserId}): {ex.Message}");
                            }
                        }

                        borrowedBooks.Add(borrowedBook);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ödünç kitap kaydı işlenirken hata (ID: {doc.Id}): {ex.Message}");
                        continue;
                    }
                }
                return borrowedBooks;
            }
            catch (Exception ex)
            {
                throw new Exception("Ödünç alınan kitaplar alınırken bir hata oluştu: " + ex.Message, ex);
            }
        }

        public async Task<BorrowedBook?> GetBorrowedBookByIdAsync(string borrowedBookId)
        {
            try
            {
                var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowedBookId);
                var snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    var data = snapshot.ToDictionary();
                    var borrowedBook = new BorrowedBook
                    {
                        Id = snapshot.Id,
                        BookId = data.GetValueOrDefault("bookId", "")?.ToString() ?? string.Empty,
                        UserId = data.GetValueOrDefault("userId", "")?.ToString() ?? string.Empty,
                        BorrowDate = (data.GetValueOrDefault("borrowDate") as Timestamp?)?.ToDateTime() ?? DateTime.UtcNow,
                        DueDate = (data.GetValueOrDefault("dueDate") as Timestamp?)?.ToDateTime() ?? DateTime.UtcNow,
                        ReturnDate = (data.GetValueOrDefault("returnDate") as Timestamp?)?.ToDateTime(),
                        IsReturned = data.GetValueOrDefault("isReturned", false) as bool? ?? false,
                        Notes = data.GetValueOrDefault("notes", "")?.ToString() ?? string.Empty,
                        CreatedAt = (data.GetValueOrDefault("createdAt") as Timestamp?)?.ToDateTime() ?? DateTime.UtcNow,
                        UpdatedAt = (data.GetValueOrDefault("updatedAt") as Timestamp?)?.ToDateTime()
                    };

                    if (!string.IsNullOrEmpty(borrowedBook.BookId))
                    {
                        borrowedBook.Book = await GetBookByIdAsync(borrowedBook.BookId);
                    }

                    if (!string.IsNullOrEmpty(borrowedBook.UserId))
                    {
                        borrowedBook.User = await GetUserAsync(borrowedBook.UserId);
                    }

                    return borrowedBook;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Ödünç kitap bilgileri alınırken bir hata oluştu: " + ex.Message, ex);
            }
        }

        public async Task AddBorrowTransactionAsync(BorrowedBook borrowBook)
        {
            try
            {
                borrowBook.CreatedAt = DateTime.UtcNow;
                var docRef = await _firestoreDb.Collection("borrowedBooks").AddAsync(new Dictionary<string, object>
                {
                    { "bookId", borrowBook.BookId ?? string.Empty },
                    { "userId", borrowBook.UserId ?? string.Empty },
                    { "borrowDate", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.BorrowDate, DateTimeKind.Utc)) },
                    { "dueDate", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.DueDate, DateTimeKind.Utc)) },
                    { "returnDate", borrowBook.ReturnDate.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.ReturnDate.Value, DateTimeKind.Utc)) : null },
                    { "isReturned", borrowBook.IsReturned },
                    { "notes", borrowBook.Notes ?? string.Empty },
                    { "createdAt", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.CreatedAt, DateTimeKind.Utc)) },
                    { "updatedAt", borrowBook.UpdatedAt.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.UpdatedAt.Value, DateTimeKind.Utc)) : null }
                });
                borrowBook.Id = docRef.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Ödünç kitap işlemi eklenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        public async Task UpdateBorrowTransactionAsync(string borrowId, BorrowedBook borrowBook)
        {
            try
            {
                borrowBook.UpdatedAt = DateTime.UtcNow;
                var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowId);
                await docRef.SetAsync(new Dictionary<string, object>
                {
                    { "bookId", borrowBook.BookId ?? string.Empty },
                    { "userId", borrowBook.UserId ?? string.Empty },
                    { "borrowDate", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.BorrowDate, DateTimeKind.Utc)) },
                    { "dueDate", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.DueDate, DateTimeKind.Utc)) },
                    { "returnDate", borrowBook.ReturnDate.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.ReturnDate.Value, DateTimeKind.Utc)) : null },
                    { "isReturned", borrowBook.IsReturned },
                    { "notes", borrowBook.Notes ?? string.Empty },
                    { "updatedAt", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.UpdatedAt.Value, DateTimeKind.Utc)) }
                }, SetOptions.MergeAll);
            }
            catch (Exception ex)
            {
                throw new Exception("Ödünç kitap işlemi güncellenirken bir hata oluştu: " + ex.Message, ex);
            }
        }

        public async Task DeleteBorrowTransactionAsync(string borrowId)
        {
            var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowId);
            await docRef.DeleteAsync();
        }

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

        public async Task AddBorrowedBookAsync(BorrowedBook borrowedBook)
        {
            var db = FirestoreDb.Create(_firestoreDb.ProjectId);
            var collection = db.Collection("borrowedBooks");
            
            borrowedBook.CreatedAt = DateTime.UtcNow;
            borrowedBook.UpdatedAt = DateTime.UtcNow;
            
            var docRef = await collection.AddAsync(new Dictionary<string, object>
            {
                { "bookId", borrowedBook.BookId ?? string.Empty },
                { "userId", borrowedBook.UserId ?? string.Empty },
                { "borrowDate", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowedBook.BorrowDate, DateTimeKind.Utc)) },
                { "dueDate", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowedBook.DueDate, DateTimeKind.Utc)) },
                { "returnDate", borrowedBook.ReturnDate.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(borrowedBook.ReturnDate.Value, DateTimeKind.Utc)) : null },
                { "isReturned", borrowedBook.IsReturned },
                { "notes", borrowedBook.Notes ?? string.Empty },
                { "createdAt", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowedBook.CreatedAt, DateTimeKind.Utc)) },
                { "updatedAt", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowedBook.UpdatedAt.Value, DateTimeKind.Utc)) }
            });
            borrowedBook.Id = docRef.Id;
        }

        public async Task UpdateBorrowedBookAsync(string borrowId, BorrowedBook borrowBook)
        {
            try
            {
                borrowBook.UpdatedAt = DateTime.UtcNow;
                var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowId);
                await docRef.SetAsync(new Dictionary<string, object>
                {
                    { "bookId", borrowBook.BookId ?? string.Empty },
                    { "userId", borrowBook.UserId ?? string.Empty },
                    { "borrowDate", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.BorrowDate, DateTimeKind.Utc)) },
                    { "dueDate", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.DueDate, DateTimeKind.Utc)) },
                    { "returnDate", borrowBook.ReturnDate.HasValue ? Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.ReturnDate.Value, DateTimeKind.Utc)) : null },
                    { "isReturned", borrowBook.IsReturned },
                    { "notes", borrowBook.Notes ?? string.Empty },
                    { "updatedAt", Timestamp.FromDateTime(DateTime.SpecifyKind(borrowBook.UpdatedAt.Value, DateTimeKind.Utc)) }
                }, SetOptions.MergeAll);
            }
            catch (Exception ex)
            {
                throw new Exception("Ödünç kitap güncellenirken bir hata oluştu: " + ex.Message, ex);
            }
        }
        #endregion

        #region Counts and Queries
        public async Task<int> GetBookCountAsync()
        {
            var snapshot = await _firestoreDb.Collection("books").GetSnapshotAsync();
            return snapshot.Count;
        }

        public async Task<int> GetUserCountAsync()
        {
            var snapshot = await _firestoreDb.Collection("users").GetSnapshotAsync();
            return snapshot.Count;
        }

        public async Task<int> GetBorrowedBookCountAsync()
        {
            var snapshot = await _firestoreDb.Collection("borrowedBooks").GetSnapshotAsync();
            return snapshot.Count;
        }

        public async Task<int> GetActiveTransactionCountAsync()
        {
            var snapshot = await _firestoreDb.Collection("borrowedBooks")
                .WhereEqualTo("IsReturned", false)
                .GetSnapshotAsync();
            return snapshot.Count;
        }

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
        public async Task<bool> BookExistsAsync(string bookId)
        {
            var docRef = _firestoreDb.Collection("books").Document(bookId);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists;
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            var docRef = _firestoreDb.Collection("users").Document(userId);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists;
        }

        public async Task<bool> BorrowedBookExistsAsync(string borrowId)
        {
            var docRef = _firestoreDb.Collection("borrowedBooks").Document(borrowId);
            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists;
        }
        #endregion
    }
}
