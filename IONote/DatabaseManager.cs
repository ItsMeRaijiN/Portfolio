using IoNote.DatabaseModels;
using IoNote.NoteModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace IoNote.AuthorizationModels
{
    static class DatabaseManager
    {
        static private ionoteContext dbContext = new();

        public static bool LogInUser(string username, ref SignedInUser user)
        {
            try
            {
                if (dbContext.users.Where(u => u.username == username).FirstOrDefault() == null)
                    return false;

                user = new SignedInUser(dbContext.users.Where(u => u.username == username).FirstOrDefault());

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool LogOutUser(ref SignedInUser user)
        {
            try
            {
                user = new SignedInUser();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DeleteUser(string username)
        {
            try
            {
                var user = dbContext.users.Where(u => u.username == username).FirstOrDefault();
                if (user == null)
                    return false;

                DeleteAccountsContent(user.userid);
                dbContext.users.Remove(user);
                dbContext.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsUsernameAvailable(string username)
        {
            var user = dbContext.users.Where(u => u.username == username).FirstOrDefault();
            if (user == null)
                return true;
            return false;
        }

        public static bool AddUser(string username, string password, string question, string answer)
        {
            try
            {
                if (!IsUsernameAvailable(username))
                    return false;
                user u = new user { username = username, password = password, question = question, answer = answer };
                dbContext.users.Add(u);
                AddFolder("Root", u.userid, null);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool CheckUser(string username, string password)
        {
            try
            {
                var user = dbContext.users.Where(u => u.username == username && u.password == password).FirstOrDefault();
                if (user != null)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool CheckUser(string username)
        {
            try
            {
                var user = dbContext.users.Where(u => u.username == username).FirstOrDefault();
                if (user != null)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetPasswordReminderQuestion(string username)
        {
            try
            {
                var user = dbContext.users.Where(u => u.username == username).FirstOrDefault();
                if (user != null)
                    return user.question;
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static bool IsAnswerValid(string username, string answer)
        {
            try
            {
                var user = dbContext.users.Where(u => u.username == username).FirstOrDefault();
                if (user != null && user.answer == answer)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ChangePassword(string username, string password)
        {
            try
            {
                var user = dbContext.users.Where(u => u.username == username).FirstOrDefault();
                if (user != null)
                {
                    user.password = password;
                    dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool AddFolder(string folderName, int userId, int? parentFolderId = null)
        {
            try
            {
                folder f = new folder {name = folderName, userid = userId, parentfolderid = parentFolderId, createdat = DateTime.Now, updatedat = DateTime.Now, };
                dbContext.folders.Add(f);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception) 
            {
                return false;
            }
        }

        public static bool AddNote(string noteName, int? folderId)
        {
            try
            {
                note n = new note { name = noteName, folderid = folderId, createdat = DateTime.Now, updatedat = DateTime.Now };
                dbContext.notes.Add(n);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    
        public static bool GetFolderId(string username, string foldername, ref int? id, int parentId)
        {
            try
            {
                int? idValue = dbContext.folders.Where(f => f.name == foldername && f.parentfolderid == parentId).First().folderid;
                if (idValue == null)
                {
                    return false;
                }
                id = idValue;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ChangeContent(int noteId, string? content, int? folderId)
        {
            try
            {
                var note = dbContext.notes.Where(n => n.noteid == noteId && n.folderid == folderId).FirstOrDefault();
                if (note != null)
                {
                    note.content = content;
                    note.updatedat = DateTime.Now;
                    dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool AddImage(byte[] imageBlob, int noteId, string name)
        {
            try
            {
                var image = new image
                {
                    name = name,
                    noteid = noteId,
                    data = imageBlob,
                    createdat = DateTime.Now
                };

                dbContext.images.Add(image);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<image> GetImagesByNoteId(int noteId)
        {
            try
            {
                return dbContext.images.Where(img => img.noteid == noteId).ToList();
            }
            catch (Exception)
            {
                return new List<image>();
            }
        }


        public static byte[] GetImage(int noteId, string name)
        {
            try
            {
                var image = dbContext.images.Where(img => img.noteid == noteId && img.name == name).FirstOrDefault();
                return image?.data;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static bool CheckImage(int noteId, string name)
        {
            try
            {
                return dbContext.images.Any(img => img.noteid == noteId && img.name == name);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ChangeImage(int noteId, string name, byte[] imageBlob)
        {
            try
            {
                var image = dbContext.images.FirstOrDefault(img => img.noteid == noteId && img.name == name);

                if (image != null)
                {
                    // Nadpisz istniejący obraz
                    image.data = imageBlob;
                    image.updatedat = DateTime.Now;
                }
                else
                {
                    // Dodaj nowy obraz
                    dbContext.images.Add(new image
                    {
                        noteid = noteId,
                        name = name,
                        data = imageBlob,
                        createdat = DateTime.Now
                    });
                }

                dbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<string> GetImagesName(int noteId)
        {
            try
            {
                // Pobierz nazwy wszystkich zdjęć przypisanych do notatki
                return dbContext.images
                    .Where(img => img.noteid == noteId)
                    .Select(img => img.name)
                    .ToList();
            }
            catch (Exception)
            {
                return new List<string>(); // Zwróć pustą listę w razie błędu
            }
        }

        public static bool DeleteImage(int noteId, string name)
        {
            try
            {
                var image = dbContext.images.FirstOrDefault(img => img.noteid == noteId && img.name == name);
                if (image != null)
                {
                    dbContext.images.Remove(image);
                    dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DeleteNote(int noteId)
        {
            try
            {
                var note = dbContext.notes.FirstOrDefault(n => n.noteid == noteId);
                if (note != null)
                {
                    foreach (var image in dbContext.images.Where(img => img.noteid == noteId))
                    {
                        dbContext.images.Remove(image);
                    }
                    dbContext.notes.Remove(note);
                    dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DeleteAccountsContent(int userid)
        {
            try
            {
                using var transaction = dbContext.Database.BeginTransaction();

                var imagesToDelete = from i in dbContext.images
                                     join n in dbContext.notes on i.noteid equals n.noteid
                                     join f in dbContext.folders on n.folderid equals f.folderid
                                     where f.userid == userid
                                     select i;
                dbContext.images.RemoveRange(imagesToDelete);

                var notesToDelete = from n in dbContext.notes
                                    join f in dbContext.folders on n.folderid equals f.folderid
                                    where f.userid == userid
                                    select n;
                dbContext.notes.RemoveRange(notesToDelete);

                var foldersToDelete = dbContext.folders.Where(f => f.userid == userid);
                dbContext.folders.RemoveRange(foldersToDelete);

                dbContext.SaveChanges();
                transaction.Commit();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DeleteFolder(int folderId)
        {
            try
            {
                var folder = dbContext.folders.FirstOrDefault(f => f.folderid == folderId);
                if (folder != null)
                {
                    var notesToDelete = dbContext.notes.Where(n => n.folderid == folderId).ToList();
                    var imagesToDelete = new List<image>();

                    foreach (var note in notesToDelete)
                    {
                        imagesToDelete.AddRange(dbContext.images.Where(img => img.noteid == note.noteid));
                    }

                    var subfoldersToDelete = dbContext.folders.Where(f => f.parentfolderid == folderId).ToList();

                    dbContext.images.RemoveRange(imagesToDelete);

                    dbContext.notes.RemoveRange(notesToDelete);

                    foreach (var subfolder in subfoldersToDelete)
                    {
                        DeleteFolder(subfolder.folderid);
                    }

                    dbContext.folders.Remove(folder);

                    dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ChangeNoteFolder(int noteId, int? newFolderId)
        {
            try
            {
                var note = dbContext.notes.FirstOrDefault(n => n.noteid == noteId);
                if (note != null)
                {
                    note.folderid = newFolderId;
                    dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ImportNoteFromJSON(string jsonPath, int userId, int? folderId)
        {
            try
            {
                JsonDocument jsonDocument = ImportExportHandler.ReadJSONFile(jsonPath);
                var note = ImportExportHandler.DeserializeNoteFromJSON(jsonDocument);
                note.folderid = folderId;
                note.createdat = DateTime.Now;
                note.updatedat = DateTime.Now;
                dbContext.notes.Add(note);

                foreach (var image in note.images)
                {
                    image.createdat = DateTime.Now;
                    image.noteid = note.noteid;
                    dbContext.images.Add(image);
                }

                dbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
