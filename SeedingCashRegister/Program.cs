using System;
using System.Collections.Generic;
using System.Linq;

namespace CashRegister
{

    public class Money
    {
        public string Denomination { get; set; }
        public decimal Value { get; set; }
    }

    public class Bin
    {
        public Money Money { get; set; }
        public int Unit { get; set; }
    }

    public class Multiples
    {
        public Money Money { get; set; }
        public decimal multiples { get; set; }
    }


    public class CashRegister
    {
        private static IList<Bin> CID { get; set; }
        private static string Status { get; set; }
        public CashRegister() { }
        public CashRegister( IList<Bin> cin)
        {
            CID = cin;
        }

        public  static List<Bin> SeedCashRegister(decimal cashLimit, Dictionary<string, decimal> denoms)
        {
            List<Bin> bins = new List<Bin>();
            List<Multiples> multiples = new List<Multiples>();
            Bin bin;
            decimal numberTimes;
            Money money;
            decimal value;

            foreach(KeyValuePair<string, decimal> denom in denoms)
            {
                money = new Money
                {
                    Denomination = denom.Key,
                    Value = denom.Value
                };
                bin = new Bin
                {
                    Money = money
                };
                var multip = new Multiples
                {
                    Money = money
                };
                bins.Add(bin);
                multiples.Add(multip);

            }
            for (int i = 0; i < bins.Count(); i++)
            {
                multiples[i].Money = bins[i].Money;
                value = bins[i].Money.Value;
                numberTimes = cashLimit/value;
                multiples[i].multiples = numberTimes;
            }
            multiples.Sort((valueX, valueY) => valueX.multiples.CompareTo(valueY.multiples));
            multiples.Reverse();
            for(int j = 0; j < multiples.Count(); j++)
            { 
                for(int i = 0; i < multiples.Count() && cashLimit > 0.00m && cashLimit > multiples[i].Money.Value; i++)
                {
                    var deno = multiples[i].Money.Denomination;
                    bin = bins.Find(x => x.Money.Denomination == deno);
                    bin.Unit += 1;
                    cashLimit -= bin.Money.Value;
                    bins[bins.IndexOf(bin)] = bin;
                }       
            }
            return bins;
        }


        private decimal CalculateValueSumOfMoney(List<Bin> moneyBins)
        {
            decimal binTotalMoneySum = 0.0m;
            foreach (var moneyBin in moneyBins)
            {
                var value = moneyBin.Money.Value;
                var unit = moneyBin.Unit;
                binTotalMoneySum += (value*unit);
            }
            return binTotalMoneySum;
        }


        private decimal Approximate(decimal value)
        {
            decimal approximatedValue;
            var strValue = value.ToString().ToList<char>();
            approximatedValue = (strValue.Count > 3) ? value + 0.01m : value ;
            return approximatedValue;
        }

        public string TransactionExecution(decimal totalPriceValue, List<Bin> tenders, List<Bin> cid)
        {
            string scenarioStatus = "";
            decimal tendersSum = CalculateValueSumOfMoney(tenders);
            decimal balance = tendersSum - totalPriceValue;
            decimal cidTotalValue = CalculateValueSumOfMoney(cid);
            cidTotalValue = Approximate(cidTotalValue);

            if (balance < 0.00m)
            {
                scenarioStatus = "Insufficent Value of Tenders: transaction is not successful";
                return scenarioStatus;
            }
            else if (balance > cidTotalValue)
            {
                scenarioStatus = "Cannot give appropriate balance: transaction is not successful";
                return scenarioStatus;
            }
            else if (balance == cidTotalValue)
            {
                scenarioStatus = "Close register";
                return scenarioStatus;
            }
            else
            {
                var balanceInBin = FindTheBalance(balance, cid);
                CID = SubtractFromCID(CID, balanceInBin);
                if (Status == "Successful") scenarioStatus = "Success transaction";
                else scenarioStatus = "Failure as there is no balance";
            }
            return scenarioStatus;
        }


        private  List<Bin> AddBackToCID(List<Bin> cid, List<Bin> bins)
        {
            List<Bin> newCid = new List<Bin>();
            foreach (var deno in cid)
            {
                foreach (var bin in bins)
                {
                    if (deno.Money.Denomination == bin.Money.Denomination)
                    {
                        deno.Unit += bin.Unit;
                        break;
                    }
                }
                newCid.Add(deno);
            }
            return newCid;
        }

        private static List<Bin> SubtractFromCID(IList<Bin> cid, List<Bin> bins)
        {
            List<Bin> newCid = new List<Bin>();
            foreach (var deno in cid)
            {
                foreach (var bin in bins)
                {
                    if (bin.Money.Denomination == deno.Money.Denomination)
                    {
                        deno.Unit -= bin.Unit;
                        break;
                    }
                }
                newCid.Add(deno);
            }
            return newCid;
        }

        // The method to make a balance from a transactionhhh

        private  List<Bin> FindTheBalance(decimal balance, List<Bin> cid)
        {
            Money tender;
            int unit = 0;
            Bin bin;
            List<Bin> bins = new List<Bin>();
            cid.Sort((valueX, valueY) => valueX.Money.Value.CompareTo(valueY.Money.Value));
            cid.Reverse();
            for (int j = 0; j < cid.Count(); j++)
            {
                for (int i = j; i < cid.Count; i++)
                {
                    tender = cid[i].Money;
                    while (balance >= tender.Value && cid[i].Unit > 0)
                    {
                        balance -= tender.Value;
                        cid[i].Unit -= 1;
                        unit += 1;
                        bin = new Bin
                        {
                            Money = tender,
                            Unit = unit
                        };
                        bins.Add(bin);
                    }

                    if (balance == 0.00m) break;
                }
                if (balance != 0.00m)
                {
                    cid = AddBackToCID(cid, bins);
                    cid.Sort((valueX, valueY) => valueX.Money.Value.CompareTo(valueY.Money.Value));
                    cid.Reverse();
                    bins.RemoveAll(b => b != null);
                }

                else
                {
                    Status = "Successful";
                    break;
                    
                }

            }
            return bins;
        }



        public class Program
        {

            static void Main(string[] args)
            {
                Dictionary<string, decimal> test = new Dictionary<string, decimal>
                {
                    {"Hundred", 100m },
                    {"Fifty", 50m },
                    {"Twenty", 20m },
                    {"Ten", 10m },
                    {"Five", 5m },
                    {"Two", 2m },
                    {"One", 1m},
                    {"50 cent", 0.5m },
                    {"twenty cent", .20m },
                    {"25 cent", .25m },
                    {"ten cent", .1m},
                    {"0.05 cent", 0.05m },
                    {"0.02 cent", 0.02m },
                    {"0.01", 0.01m }
                };
                var bins = SeedCashRegister(50m, test);
                foreach (var bin in bins)
                {
                    Console.WriteLine("{0}\t {1}\t {2}", bin.Money.Denomination, bin.Money.Value, bin.Unit);

                }
                var mon = new Money
                {
                    Denomination = "50 Cent",
                    Value = 0.5m
                };
                var mo = new Money
                {
                    Denomination = "One",
                    Value = 1m
                };

                var li = new List<Bin>
                {
                    new Bin
                    {
                        Money = mon,
                        Unit = 2
                    },
                    new Bin
                    {
                        Money = mo,
                        Unit = 3
                    }


                };

                var cashRegister = new CashRegister(bins);
                var str = cashRegister.TransactionExecution(10m, li, bins);
                Console.WriteLine(str);
                Console.ReadKey();
            }
        }
    }
}
