using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IoNote.DatabaseModels;

public partial class user
{
    [Key]
    public int userid { get; set; }

    public string username { get; set; } = null!;

    public string password { get; set; } = null!;

    public string question { get; set; } = null!;

    public string answer { get; set; } = null!;
}
