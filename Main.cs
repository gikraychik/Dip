using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Diploma
{
	class Operation
	{
		public Operation(int amount, DateTime dt)
		{
			this.amount = amount;
			this.dt = dt;
		}
		public string ToString()
		{
			return amount.ToString() + " " + dt.ToString();
		}
		public int amount;
		public DateTime dt;		// время, в которое нужно осуществить операцию
	}
	class State
	{
		public State(int pos, double money, double last_price, double last_volume)
		{
			this.pos = pos;
			this.money = money;
			this.last_price = last_price;
			this.last_volume = last_volume;
		}
		public string ToString()
		{
			return pos + " " + money + " " + last_price + " " + last_volume;
		}
		public int pos;
		public double money;
		public double last_price;
		public double last_volume;
	}
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
			his = new List<State>();
			his.Add (this.state);
		}
		public Robot(double money, List<Data> d0)
		{
			this.pos = 0;
			this.money = money;
			this.dcn = (x => 0);
			this.d = d0;
			his = new List<State>();
			his.Add(this.state);
		}
		public Robot(double money, List<Data> d0, int last_data, List<double> st_params, double comm, int asset_type, int time_offset_ms)
		{
			his = new List<State>();
			d = new List<Data>();
			this.strat_params = new List<double>();
			ops = new Queue<Operation>();
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
			this.time_offset_ms = time_offset_ms;
			his.Add(this.state);
		}
		public int Trade(int amount, double price, bool is_last_operation)
		{
			//if (is_last_operation) { Console.WriteLine("Len : " + ops.Count); }
			if (amount == 0 && ops.Count == 0 && !is_last_operation) { return 0; }
			int a = 0;
			if ((ops.Count == 0 && !is_last_operation) || (pos != 0 && is_last_operation && ops.Count == 0))
			{
				ops.Enqueue(new Operation(amount, cur_dt + new TimeSpan(0, 0, 0, 0, delay)));
				//Console.WriteLine ("PEEK : " + ops.Peek().ToString() + " amount : " + amount + " last_price : " + last_price);
				//return 0;
			}
			if (ops.Count > 0)
			{
				Operation o = ops.Peek();
				if (o.dt <= cur_dt)			// можно исполнять торговую операцию
				{
					amount = o.amount;
					if (is_last_operation)
					{
						if (pos != 0) { a = -pos; }
						else { a = 0; }
					}
					else { a = (int)(money < amount * price ? CalcContracts(money, price) : amount); }
					if (a == 0)			// не успели совершить сделку, ошибка биржи
					{
						ops.Dequeue();
						return 0;
					}
					pos += a;
					money -= a * price;
					money -= Math.Abs(CalcCommission(a, price));
					last_price = price;
					//Console.WriteLine("last_price : " + price);
					ops.Dequeue();
					return a;
				}
				return 0;
			}
			else { return 0; }
		}
		public void Move(Data d)
		{
			bool is_last = (count >= last_data ? true : false);
			//if (is_last) { Console.WriteLine("LAST_OPERATION : " + d.ToString()); }
			count++;
			this.d.Add(d);
			int decision = dcn(this.d);
			//if (decision != 0) { Console.WriteLine("Data : " + d.ToString()); }
			//Console.WriteLine("decision : " + decision + " last_price : " + last_price);
			int amount = Trade (decision, d.v, is_last);
			if (amount != 0)
			{
				his.Add(this.state);
			}
		}
		public List<double> yields()
		{
			List<double> res = new List<double>();
			double m = his[0].money;
			for (int i = 1; i < his.Count; ++i)
			{
				if (his[i].pos == 0)
				{
					res.Add((his[i].money - m) / m);
					m = his[i].money;
				}
			}
			return res;
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
			Console.WriteLine(this.state.ToString());
		}
		// mode == 0 - asset
		// mode == 1 - forts
		public DateTime cur_dt
		{
			get { return d[d.Count-1].dt; }
		}
		public double R2()
		{
			double res = 0.0;
			return res;
		}
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
			foreach (State x in his)
			{
				Console.WriteLine(x.ToString());
			}
		}
		public double last_price
		{
			get { return l_price; }
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
		private Queue<Operation> ops;		// торговые операции, ожидающие исполнения
		private int time_offset_ms;
		private double l_price;
		private int position;
		private double mon;
		private Func<List<Data>, int> f;
		private List<Data> d;
		private List<State> his;
		private int count;
		private int last_data;
		private double commission_rate;
		// 0 - asset
		// 1 - futures
		private int asset_type;
		public int delay
		{
			get { return time_offset_ms; }
			set { time_offset_ms = value; }
		}
		public int last_volume
		{
			get { return 0; }
		}
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
		public State state
		{
			get
			{
				return new State(pos, money, last_price, last_volume);
				/* ArrayList x = new ArrayList();
				x.Add(pos); x.Add(money); x.Add(dcn);
				return x; */
			}
		}
		public double AY
		{
			get { return (state.money - his[0].money) / his[0].money; }
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
			int num = 0;
			double comm = 0.0001;
			double step = 0.0002;
			foreach (string path in Directory.GetFiles("Data/Ticks"))
			{
				List<Data> all_data = ReadData (path);
				List<Data> d = FactorSeconds(all_data);
				//DateTime dt = new DateTime(2000, 1, 1, 9, 59, 59, 0) + new TimeSpan(0, 0, 0, 0, 1000);
				//Console.WriteLine(dt.ToString());
				//return;
				/*for (int i = 0; i < d.Count; ++i)
				{
					//Console.WriteLine (all_data[i].dt + " " + all_data[i].v + " " + all_data[i].volume);
					Console.WriteLine(d[i].dt + " " + d[i].v + " " + d[i].volume);
				}*/
				int count = d.Count-1;
				/*for (int i = 0; i < count+1; ++i)
				{
					Console.WriteLine(d[i].ToString());
				}*/
				Robot r = new Robot(1000, null, count - 1, new List<double>(new double[] { step }), comm, 0, 1000);
				for (int i = 0; i < count+1; ++i)
				{
					r.Move (d[i]);
				}
				List<double> y = r.yields();
				double ay = r.AY;
				//Console.WriteLine("History:");
				//r.PrintHistory();
				//Console.WriteLine("AY = " + r.AY);
				StreamWriter wr = new StreamWriter("Strat1/asset/1000/" + num + ".txt");
				for (int i = 0; i < y.Count; ++i)
				{
					wr.WriteLine(y[i]*100);
				}
				wr.WriteLine(ay);
				Console.WriteLine(ay);
				//r.PrintHistory();
				wr.Close();
				//break;
			}
		}
	}
}