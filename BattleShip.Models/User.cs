namespace BattleShip.Models;

public class User
{
    private int id;
    private string username;

    public User(string username)
    {
        this.username = username;
    }

    public void setId(int id){
        this.id = id;
    }

    public int getId(){
        return this.id;
    }

    public string getUsername(){
        return this.username;
    }
}
