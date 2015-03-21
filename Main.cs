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
		public string ToString()
		{
			return dt.ToString() + " " + v + " " + volume;
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
		public Robot(double money, List<Data> d0, int last_data, List<double> st_params, double comm, int asset_type)
		{
			his = new List<ArrayList>();
			d = new List<Data>();
			this.strat_params = new List<double>();
			this.pos = 0;
			this.money = money;
			this.dcn = (x => 0);
			if (d0 != null)
				this.d = d0;
			this.dcn = this.Strat1;
			this.last_data = last_data;
			this.l_price = 0.0;
			this.pars = st_params;
			this.commission_rate = comm;
			this.asset_type = asset_type;
			his.Add(this.state);
		}
		public int Trade(int amount, double price, bool is_last_operation)
		{
			int a = 0;
			if (is_last_operation)
			{
				if (pos != 0)		// открыта позиция, нужно ее закрыть
				{
					a = -pos;
				}
				else { a = 0; }
			}
			else { a = (int)(money < amount * price ? CalcContracts(money, price) : amount); }
			if (a == 0) return 0;
			pos += a;
			money -= a * price;
			money -= Math.Abs(CalcCommission(a, price));
			his.Add(this.state);
			if (a > 0) last_price = price;
			return a;
		}
		private int CalcContracts(double m, double p)
		{
			int n = (int)(m / p);
			double c = CalcCommission(n, p);
			while (m - c - p * n < 0)
			{
				n--;
				c = CalcCommission(n, p);
			}
			return n;
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
		// mode == 0 - asset
		// mode == 1 - forts
		private double CalcCommission(int amount, double price)
		{
			if (type == 0)		// asset
			{
				return amount * price * commission;
			}
			else
			{
				return amount * commission;
			}
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
		private List<double> strat_params;
		public List<double> pars
		{
			get { return strat_params; }
			set { strat_params = value; }
		}
		public double commission
		{
			get { return commission_rate; }
		}
		private double l_price;
		private int position;
		private double mon;
		private Func<List<Data>, int> f;
		private List<Data> d;
		private List<ArrayList> his;
		private int count;
		private int last_data;
		private double commission_rate;
		// 0 - asset
		// 1 - futures
		private int asset_type;
		public int type
		{
			get { return asset_type; }
		}
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
		public double AY
		{
			get { return ((double)state[1] - (double)his[0][1]) / (double)his[0][1]; }
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
		private int Strat1(List<Data> d)
		{
			if (d.Count == 1) return 0;
			double l = last_price;
			double a = pars[0];		// param
			int p = pos;
			//double m = money;
			int last_index = d.Count - 1;
			double price = d[last_index].v;
			if (p == 0)		// если мгновенное отклонение большое, открыть позицию
			{
				double ds = (d[last_index].v - d[last_index - 1].v) / d[last_index - 1].v;
				if (Math.Abs (ds) >= a)		// относительное приращение больше заданного
				{
					return (ds < 0.0 ? 1 : -1) * (int)(money / price);
				}
				return 0;
			}
			else 			// закрыть позицию
			{
				//return -p;
				double c = Math.Abs(CalcCommission(p, price));
				if ((price - l) * p - c > 0) { return -p; }
				else if (((price - l) * p - c) / (l * p) <= -0.01) { return -p; }
				else { return 0; }
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
			/*for (int i = 0; i < d.Count; ++i)
			{
				//Console.WriteLine (all_data[i].dt + " " + all_data[i].v + " " + all_data[i].volume);
				Console.WriteLine(d[i].dt + " " + d[i].v + " " + d[i].volume);
			}*/
			int count = d.Count;
			for (int i = 0; i < count; ++i)
			{
				Console.WriteLine(d[i].ToString());
			}
			double comm = 0.0001;
			//double comm = 0.0;
			double step = 0;
			double h = 0.0001;
			List<string> lines = new List<string>();
			while (step < 0.01)
			{
				Robot r = new Robot(1000, null, count - 1, new List<double>(new double[] { step }), comm, 0);
				for (int i = 0; i < count; ++i)
				{
					r.Move (d[i]);
				}
				//Console.WriteLine("History:");
				//r.PrintHistory();
				//Console.WriteLine("AY = " + r.AY);
				lines.Add(step.ToString() + " " + r.AY);
				step += h;
			}
			StreamWriter wr = new StreamWriter("Strat1/SBER.txt");
			foreach(string s in lines)
			{
				wr.WriteLine(s);
			}
			wr.Close();
		}
	}
}