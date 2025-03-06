using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IoNote.DatabaseModels;
using System.IO;

namespace IoNote.NoteModels
{
    internal class ImportExportHandler
    {
        static public JsonDocument ReadJSONFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Nie znaleziono pliku JSON");

            string jsonString = File.ReadAllText(path);
            return JsonDocument.Parse(jsonString);
        }

        //static public note DeserializeNoteFromJSON(JsonDocument json)
        //{
        //    note tempNote = new note();
        //    foreach (JsonProperty item in json.RootElement.EnumerateObject())
        //    {
        //        switch (item.Name)
        //        {
        //            case "noteid":
        //                tempNote.noteid = item.Value.GetInt32();
        //                break;
        //            case "name":
        //                tempNote.name = item.Value.GetString();
        //                break;
        //            case "content":
        //                tempNote.content = item.Value.GetString();
        //                break;
        //            case "background":
        //                tempNote.background = item.Value.GetInt32();
        //                break;
        //            case "folderid":
        //                tempNote.folderid = item.Value.GetInt32();
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    return tempNote;
        //}

        static public note DeserializeNoteFromJSON(JsonDocument json)
        {
            note tempNote = new note();
            var root = json.RootElement;

            if (!root.TryGetProperty("name", out JsonElement nameElement))
                throw new JsonException("Brak wymaganego pola 'name'");

            tempNote.name = nameElement.GetString();

            if (root.TryGetProperty("content", out JsonElement contentElement))
                tempNote.content = contentElement.GetString();

            if (root.TryGetProperty("background", out JsonElement bgElement))
                tempNote.background = bgElement.GetInt32();

            if (root.TryGetProperty("folderid", out JsonElement folderidElement))
                tempNote.folderid = folderidElement.GetInt32();

            if (root.TryGetProperty("images", out JsonElement imagesElement))
            {
                foreach (JsonElement item in imagesElement.EnumerateArray())
                {
                    try
                    {
                        image tempImage = new image();

                        if (item.TryGetProperty("name", out JsonElement imageNameElement))
                            tempImage.name = imageNameElement.GetString();
                        else
                            continue;  

                        if (item.TryGetProperty("data", out JsonElement dataElement))
                        {
                            try
                            {
                                tempImage.data = dataElement.GetBytesFromBase64();
                            }
                            catch
                            {
                                continue;  // Pomijamy niepoprawne dane Base64
                            }
                        }

                        tempNote.images.Add(tempImage);
                    }
                    catch
                    {
                        continue;  // Pomijamy niepoprawne obrazy
                    }
                }
            }

            return tempNote;
        }

 

        static public List<image> DeserializeImagesFromJSON(JsonDocument json)
        {
            List<image> tempImages = new List<image>();

            if (!json.RootElement.TryGetProperty("images", out JsonElement imagesElement))
                return tempImages;  // Brak obrazów to nie błąd

            foreach (JsonElement item in imagesElement.EnumerateArray())
            {
                try
                {
                    image tempImage = new image();

                    if (item.TryGetProperty("name", out JsonElement nameElement))
                        tempImage.name = nameElement.GetString();
                    else
                        continue;  // Pomijamy obrazy bez nazwy

                    if (item.TryGetProperty("data", out JsonElement dataElement))
                    {
                        try
                        {
                            tempImage.data = dataElement.GetBytesFromBase64();
                        }
                        catch
                        {
                            continue;  // Pomijamy niepoprawne dane Base64
                        }
                    }

                    tempImages.Add(tempImage);
                }
                catch
                {
                    continue;  // Pomijamy niepoprawne obrazy
                }
            }

            return tempImages;
        }

        static public string SerializeNoteToJSON(note note, List<image> imageList)
        {
            var noteDict = new Dictionary<string, object>
            {
                { "noteid", note.noteid },
                { "name", note.name },
                { "content", note.content },
                { "background", note.background },
                { "folderid", note.folderid },
                { "images", imageList.Select(img => new
                    {
                        img.imageid,
                        img.noteid,
                        img.name,
                        data = Convert.ToBase64String(img.data)
                    }).ToList()
                }
            };

            return JsonSerializer.Serialize(noteDict);
        }

        static public bool WriteJSONFile(string path, string jsonString)
        {
            try
            {
                File.WriteAllText(path, jsonString);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
