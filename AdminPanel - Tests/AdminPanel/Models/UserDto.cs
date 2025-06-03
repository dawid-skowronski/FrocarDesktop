using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using ReactiveUI;

namespace AdminPanel.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsCurrentUser { get; set; }

        public ReactiveCommand<UserDto, Unit> EditCommand { get; set; }
        public ReactiveCommand<UserDto, Unit> DeleteCommand { get; set; }
    }
}