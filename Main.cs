using System;
using System.Collections;
using System.Collections.Generic;


namespace Diploma
{
	class Robot
	{
		private int position;
		private double mon;
		private Func<IList<double>, int> f;
		public int pos
		{
			get { return position; }
			set {position = value;}
		}
		public double money
		{
			get { return mon; }
			set { mon = value; }
		}
		public Func<IList<double>, int> dcn
		{
			get { return f; }
			set { f = value; }
		}
		
	}
	class MainClass
	{
		public static void Main (string[] args)
		{
			Robot r = new Robot();
			//r.f = (x => x + 2);
			List<double> y = new List<double>();
			y.Add(-1.5);
			y.Add(20.0);
			r.dcn = (x => (int)x[0]);
			Console.WriteLine(r.dcn(y));
			//Console.WriteLine("Hello");
		}
	}
}
