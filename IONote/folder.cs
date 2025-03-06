using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IoNote.DatabaseModels;

public partial class folder
{
    [Key]
    public int folderid { get; set; }

    public string name { get; set; } = null!;

    public int userid { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime createdat { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? updatedat { get; set; }

    public int? parentfolderid { get; set; }

    [InverseProperty("parentfolder")]
    public virtual ICollection<folder> Inverseparentfolder { get; set; } = new List<folder>();

    [InverseProperty("folder")]
    public virtual ICollection<note> notes { get; set; } = new List<note>();

    [ForeignKey("parentfolderid")]
    [InverseProperty("Inverseparentfolder")]
    public virtual folder? parentfolder { get; set; }
}
