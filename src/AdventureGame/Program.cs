﻿namespace AdventureGame;

public class Program
{
	public static void Main()
	{
		Room r1 = new Room();


		r1.SetDescription("Room 1");
        
		Room r2 = new Room();

		r2.SetLamp(true);

		Console.WriteLine(r1);
		Console.WriteLine(r2);
	}
}