﻿namespace AdventureGame;

public class Program
{
	public static void Main()
	{
		Adventurer a = new Adventurer();
        
		Adventurer b = new Adventurer();
		b.SetLamp(true);
		b.SetKey(true);
	
		Console.WriteLine(a);
		Console.WriteLine(b);
	}
}