using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Diploma
{
	class Data
	{
		public Data()
		{
			//this.dt = null;
			this.v = 0.0;
			this.volume = 0;
		}
		public Data(double v)
		{
			//this.dt = null;
			this.v = v;
			this.volume = 0;
		}
		public Data(string dt_str, double v, int volume)
		{
			string[] parts = dt_str.Split (' ');
			string[] p1 = parts[0].Split('/');
			string[] p2 = parts[1].Split(':');
			dt = new DateTime(int.Parse(p1[2])+2000, int.Parse (p1[1]), int.Parse (p1[0]), int.Parse (p2[0]), int.Parse (p2[1]), int.Parse (p2[2]));
			this.v = v;
			this.volume = volume;
		}
		public Data(DateTime dt, double v, int volume)
		{
			this.dt = dt;
			this.v = v;
			this.volume = volume;
		}
		       
		public DateTime dt;
		public double v;
		public int volume;
	}
	class Robot
	{
		public Robot()
		{
			this.pos = 0; this.money = 0.0; this.dcn = (x => 0);
			his = new List<ArrayList>();
			his.Add (this.state);
		}
		public Robot(double money, List<Data> d0)
		{
			this.pos = 0;
			this.money = money;
			this.dcn = (x => 0);
			this.d = d0;
			his = new List<ArrayList>();
			his.Add(this.state);
		}
		public Robot(double money, List<Data> d0, int last_data)
		{
			his = new List<ArrayList>();
			d = new List<Data>();
			this.pos = 0;
			this.money = money;
			this.dcn = (x => 0);
			if (d0 != null)
				this.d = d0;
			this.dcn = this.TestStrat;
			this.last_data = last_data;
			this.l_price = 0.0;
			his.Add(this.state);
		}
		public int Trade(int amount, double price, bool is_last_operation)
		{
			int a = 0;
			if (is_last_operation)
			{
				//Console.WriteLine("!!!!!!!!!!!!!!!!!!");
				//Console.ReadKey();
				if (pos != 0)		// открыта позиция, нужно ее закрыть
				{
					a = -pos;
				}
				else { a = 0; }
			}
			else { a = (int)(money < amount * price ? money / price : amount); }
			if (a == 0) return 0;
			pos += a;
			money -= a * price;
			his.Add(this.state);
			if (a > 0) last_price = price;
			return a;
		}
		public void PrintState()
		{
			ArrayList x = this.state;
			Console.WriteLine(x[0] + " " + x[1] + " func");
		}
		public void Move(Data d)
		{
			bool is_last = count == last_data ? true : false;
			//Console.WriteLine("Len: " + count.ToString() + " " + last_data.ToString());
			count++;
			this.d.Add(d);
			ArrayList st = this.state;
			int decision = dcn(this.d);
			//Console.WriteLine ("decision: " + decision);
			//if (is_last && pos == 0) { return; }
			Trade (decision, d.v, is_last);
		}
		public void PrintHistory()
		{
			foreach (ArrayList x in his)
			{
				Console.WriteLine (x[0] + " " + x[1] + " func");
			}
		}
		public double last_price
		{
			get { return pos == 0 ? 0 : l_price; }
			set { l_price = value; }
		}
		private double l_price;
		private int position;
		private double mon;
		private Func<List<Data>, int> f;
		private List<Data> d;
		private List<ArrayList> his;
		private int count;
		private int last_data;
		public int pos
		{
			get { return position; }
			set { position = value; }
		}
		public double money
		{
			get { return mon; }
			set { mon = value; }
		}
		public Func<List<Data>, int> dcn
		{
			get { return f; }
			set { f = value; }
		}
		public ArrayList state
		{
			get {
				ArrayList x = new ArrayList();
				x.Add(pos); x.Add(money); x.Add(dcn);
				//Console.WriteLine(x[1]);
				return x;
			}
		}
		private int TestStrat(List<Data> d)
		{
			double l = last_price;
			int p = pos;
			double m = money;
			double price = d[d.Count-1].v;
			//Console.WriteLine(l.ToString() + " " + price.ToString() + " " + p);
			if (p < 0)		// short
			{
				if (price < l)
				{
					return -p;
				}
				else { return 0; }
			}
			else if (p > 0)	// long or 0
			{
				Console.WriteLine(l.ToString() + " " + price.ToString() + " " + p);
				if (price > l)
				{
					return -p;
				}
				else { return 0; }
			}
			else
			{
				return (int)(m / price);
			}
		}
	}
	class MainClass
	{
		public static List<Data> ReadData(string Path)
		{
			List<Data> res = new List<Data>();
			string[] lines = File.ReadAllLines(Path);
			for (int i = 1; i < lines.Length; ++i)
			{
				string s = lines[i];
				string[] parts = s.Split (' ');
				Data d = new Data(parts[2] + " " + parts[3], double.Parse(parts[4]), int.Parse(parts[5]));
				res.Add(d);
			}
			return res;
		}
		public static List<Data> FactorSeconds(List<Data> x)
		{
			List<Data> d = new List<Data>();
			int i = 0;
			while (i < x.Count)
			{
				DateTime dt = x[i].dt;
				int volume = 0;
				double v = 0.0;
				while (x[i].dt == dt)
				{
					v += x[i].v * x[i].volume;
					volume += x[i].volume;
					i += 1;
					if (i == x.Count) break;
				}
				v /= volume;
				d.Add(new Data(dt, v, volume));
			}
			return d;
		}
		public static void Main (string[] args)
		{
			List<Data> all_data = ReadData ("SBER.txt");
			List<Data> d = FactorSeconds(all_data);
			for (int i = 0; i < 8; ++i)
			{
				//Console.WriteLine (all_data[i].dt + " " + all_data[i].v + " " + all_data[i].volume);
				Console.WriteLine(d[i].dt + " " + d[i].v + " " + d[i].volume);
			}
			
			/*Robot r = new Robot(1000, null, 8-1);
			//Console.WriteLine(r.last_price);
			//r.Move(all_data[0]);
			for (int i = 0; i < 8; ++i)
			{
				r.Move(all_data[i]);
			}
			Console.WriteLine("History:");
			r.PrintHistory();
			Console.WriteLine("State:");
			r.PrintState();*/
		}
	}
}
