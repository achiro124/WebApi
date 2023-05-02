namespace UsersWebApi.Data
{
    public static class UsersDb
    {
        public static List<UserDTO> userList = new List<UserDTO>
            {
                new UserDTO {Login = "Admin1", Password = "123" },
                new UserDTO {Login = "Admin2", Password = "321" },
                new UserDTO {Login = "Admin3", Password = "123" },
            };
    }
}
