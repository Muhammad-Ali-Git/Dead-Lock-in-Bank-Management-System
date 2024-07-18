using System;
using System.Diagnostics;
using System.Threading;

class Ali
{
    public static void Main()
    {
        Stopwatch SW = Stopwatch.StartNew();

        bool isDeadlocked = false;

        Console.WriteLine("This program manages bank account of a user.");
        Console.WriteLine("---------------------------------------------------------------------------------------------------------");

        Console.WriteLine("Main Started!");
        Console.WriteLine("---------------------------------------------------------------------------------------------------------");

        Account A = new Account (101, 5000);
        Account B = new Account (102, 3000);
        
        AccountManager AMA = new AccountManager (A, B, 1000, SW, ref isDeadlocked);
        Thread T1 = new Thread (AMA.Transfer);
        T1.Name = "T1";

        AccountManager AMB = new AccountManager (B, A, 1000, SW, ref isDeadlocked);
        Thread T2 = new Thread(AMB.Transfer);
        T2.Name = "T2";

        Thread monitoringThread = new Thread(() => Monitor(SW, ref isDeadlocked));
        monitoringThread.Start();

        T1.Start();
        T2.Start();

        T1.Join();
        T2.Join();

        Console.WriteLine("---------------------------------------------------------------------------------------------------------");
        Console.WriteLine("Main Ended!");

        Console.WriteLine("---------------------------------------------------------------------------------------------------------");
        Console.WriteLine("Total elapsed time: {0} ms", SW.ElapsedMilliseconds);
        monitoringThread.Abort();
    }

    static void Monitor(Stopwatch stopwatch, ref bool isDeadlocked)
    {
        while (stopwatch.IsRunning && !isDeadlocked)
        {
            Console.WriteLine("Elapsed time: {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("---------------------------------------------------------------------------------------------------------");
            Thread.Sleep(1000); // Check every second
        }
    }
}

public class Account
{
    double _balance;
    int _id;

    public Account (int id, double balance)
    {
        this._id = id;
        this._balance = balance;
    }

    public int id
    {
        get
        {
            return _id;
        }
    }

    public void Withdraw (double amount)
    {
        _balance = _balance - amount;
    }

    public void Deposit (double amount)
    {
        _balance = _balance + amount;
    }
}

public class AccountManager
{
    Account _fromAccount;
    Account _toAccount;
    double _TransferAmount;
    Stopwatch _stopwatch;
    private bool _isDeadlocked;


    public AccountManager(Account fromAccount, Account toAccount, double TransferAmount, Stopwatch stopwatch, ref bool isDeadlocked)
    {
        this._fromAccount = fromAccount;
        this._toAccount = toAccount;
        this._TransferAmount = TransferAmount;
        this._stopwatch = stopwatch;
        this._isDeadlocked = isDeadlocked;
    }

    public void Transfer()
    {
        Console.WriteLine("{0} trying to acquire lock on {1}", Thread.CurrentThread.Name, _fromAccount.id.ToString());
        Console.WriteLine("---------------------------------------------------------------------------------------------------------");
        lock (_fromAccount)
        {
            Console.WriteLine("{0} acquired lock on {1}", Thread.CurrentThread.Name, _fromAccount.id.ToString());
            Console.WriteLine("---------------------------------------------------------------------------------------------------------");

            Console.WriteLine("{0} suspend for 1s.", Thread.CurrentThread.Name);
            Console.WriteLine("---------------------------------------------------------------------------------------------------------");
            Thread.Sleep(1000);

            Console.WriteLine("{0} back in action and trying to acquire lock on {1}", Thread.CurrentThread.Name, _toAccount.id.ToString());
            Console.WriteLine("---------------------------------------------------------------------------------------------------------");

            Console.WriteLine("The Code is Dead-Locked at {0} ms", _stopwatch.ElapsedMilliseconds);
            _stopwatch.Stop();
            
            lock (_toAccount)
            {
                _fromAccount.Withdraw(_TransferAmount);
                _toAccount.Deposit(_TransferAmount);
            }
        }
    }
}