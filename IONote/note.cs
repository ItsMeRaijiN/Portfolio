using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IoNote.DatabaseModels;

public partial class note
{
    [Key]
    public int noteid { get; set; }

    public string name { get; set; } = null!;

    public string? content { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime createdat { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? updatedat { get; set; }

    public int? background { get; set; }

    public int? folderid { get; set; }

    [ForeignKey("folderid")]
    [InverseProperty("notes")]
    public virtual folder? folder { get; set; }

    [InverseProperty("note")]
    public virtual ICollection<image> images { get; set; } = new List<image>();
}
