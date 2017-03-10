using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace LiquidPlayer.Exec
{
    public class VirtualMachine
    {
        private int loadBX(SmartPointer bx, int[] stack, int[] dataSpace)
        {
            if (bx.LoAddress == -1)
            {
                return stack[bx.HiAddress];
            }
            else if (bx.LoAddress == 0)
            {
                return dataSpace[bx.HiAddress];
            }
            else if (bx.LoAddress > 0)
            {
                return Program.Exec.ObjectManager[bx.LoAddress].DataSpace[bx.HiAddress];
            }

            throw new NotImplementedException();
        }

        private void saveBX(SmartPointer bx, int[] stack, int[] dataSpace, int data)
        {
            if (bx.LoAddress == -1)
            {
                stack[bx.HiAddress] = data;
                return;
            }
            else if (bx.LoAddress == 0)
            {
                dataSpace[bx.HiAddress] = data;
                return;
            }
            else if (bx.LoAddress > 0)
            {
                Program.Exec.ObjectManager[bx.LoAddress].DataSpace[bx.HiAddress] = data;
                return;
            }

            throw new NotImplementedException();
        }

        public int Run(int id, int startPC = 0, int[] m1 = null, int m2 = 0)
        {
            var objectManager = Program.Exec.ObjectManager;

            // Managers
            var longManager = Program.Exec.LongManager;
            var doubleManager = Program.Exec.DoubleManager;
            var stringManager = Program.Exec.StringManager;

            // Tasks
            int taskId;
            Liquid.Task task;

            // PCB
            Liquid.Task.ProcessControlBlock PCB;

            // Program Counter and Data Segment
            int pc;
            int ds;

            // Registers
            int a0;
            int a1;
            long c0;
            long c1;
            double d0;
            double d1;
            SmartPointer bx = new SmartPointer();

            // Stack
            int[] stack;
            int sp;
            int bp;
            int sz;

            // Debugger
            int lp;
            int ln;
            int fn;

            // Interrupts
            bool irqEnabled;
            int irqCount;

            // Timers
            long lastTime;
            long now;
            long quantum;
            long elapsed;

            // Tables
            string[] fileTable;
            string[] stringTable;
            int[] aliasTable;

            // Code
            int[] codeSpace;

            // Data
            int[] dataSpace;

            // Task State
            bool isWaiting;

            // VM helper variables
            LiquidClass liquidClass;
            int data;
            int count;
            int index;
            int address;
            FunctionDelegate function;

            if (startPC == 0)
            {
                taskId = id;
                task = objectManager[taskId].LiquidObject as Liquid.Task;

                PCB = task.PCB;

                pc = PCB.PC;
                ds = PCB.DS;

                a0 = PCB.A0;
                a1 = PCB.A1;
                c0 = PCB.C0;
                c1 = PCB.C1;
                d0 = PCB.D0;
                d1 = PCB.D1;
                bx = PCB.BX;

                stack = task.Stack;
                sp = PCB.SP;
                bp = PCB.BP;
                sz = PCB.SZ;

                id = PCB.ID;

                lp = PCB.LP;
                ln = PCB.LN;
                fn = PCB.FN;

                irqEnabled = true;
                irqCount = 64;

                quantum = PCB.Quantum;
                elapsed = 0;
            }
            else
            {
                taskId = objectManager[id].TaskId;
                task = objectManager[taskId].LiquidObject as Liquid.Task;

                PCB = task.PCB;

                pc = startPC;
                ds = id;

                a0 = 0;
                a1 = 0;
                c0 = 0L;
                c1 = 0L;
                d0 = 0d;
                d1 = 0d;
                bx.Address = 0L;

                if (m1 == null)
                {
                    stack = new int[Program.VM_STACK_SIZE];
                    sp = 0;
                    bp = 0;
                    sz = stack.Length;
                }
                else
                {
                    stack = m1;
                    sp = m2;

                    sp++;
                    stack[sp] = 0;
                    sp++;
                    stack[sp] = 0;
                    sp++;
                    stack[sp] = 0;
                    sp++;
                    stack[sp] = 0;

                    bp = sp;
                    sz = stack.Length;
                }

                lp = 0;
                ln = 0;
                fn = 0;

                irqEnabled = false;
                irqCount = 0;

                quantum = 0;
                elapsed = 0;
            }

            fileTable = task.FileTable;
            stringTable = task.StringTable;
            aliasTable = task.AliasTable;

            codeSpace = task.CodeSpace;
            dataSpace = task.DataSpace;

            isWaiting = (PCB.State == Liquid.Task.ProcessState.Waiting) ? true : false;

        Recover:

            lastTime = Program.AtomicClock;

        Continue:

            Debug.Assert(sp >= 0);
            
            switch ((pCode)codeSpace[pc])
            {
                case pCode.Halt:
                    pc = 0;
                    ln = 0;
                    fn = 0;
                    goto Done;
                case pCode.Info:
                    lp = codeSpace[pc + 1];
                    ln = codeSpace[pc + 2];
                    fn = codeSpace[pc + 3];
                    pc += 4;
                    break;
                case pCode.IRQ:
                    irqCount--;

                    if (irqCount <= 0)
                    {
                        irqCount = 64;

                        now = Program.AtomicClock;
                        elapsed = now - lastTime;

                        if (elapsed >= 5000000)
                        {
                            task.Raise(ExceptionCode.Timeout);
                            pc++;
                            goto Done;
                        }

                        if (irqEnabled)
                        {
                            if (elapsed >= quantum)
                            {
                                pc++;
                                goto Done;
                            }
                        }
                    }

                    pc++;
                    break;
                case pCode.BufferA0:
                    throw new NotImplementedException();
                case pCode.BufferD0Float:
                    throw new NotImplementedException();
                case pCode.Unbuffer:
                    throw new NotImplementedException();
                case pCode.Alloc:
                    count = codeSpace[pc + 1];
                    if (count != 0)
                    {
                        if (sp + count >= sz)
                        {
                            task.Raise(ExceptionCode.StackOverflow);
                            pc += 2;
                            goto Done;
                        }
                        for (index = 1; index <= count; index++)
                        {
                            sp++;
                            stack[sp] = 0;
                        }
                    }
                    pc += 2;
                    break;
                case pCode.DecrSP:
                    sp -= codeSpace[pc + 1];
                    pc += 2;
                    break;
                case pCode.PackBX:
                    a0 = longManager.New(id, bx.Address) ;
                    pc++;
                    break;
                case pCode.UnpackBX:
                    bx.Address = longManager[a0];
                    pc++;
                    break;
                case pCode.Unused:
                    throw new NotImplementedException();
                case pCode.PointGlobal:
                    bx.LoAddress = 0;
                    bx.HiAddress = codeSpace[pc + 1];
                    pc += 2;
                    break;
                case pCode.PointMacro:
                    bx.LoAddress = ds;
                    bx.HiAddress = codeSpace[pc + 1];
                    pc += 2;
                    break;
                case pCode.PointBP:
                    bx.LoAddress = -1;
                    bx.HiAddress = bp + codeSpace[pc + 1];
                    pc += 2;
                    break;
                case pCode.PointSP:
                    bx.LoAddress = -1;
                    bx.HiAddress = sp + codeSpace[pc + 1];
                    pc += 2;
                    break;
                case pCode.PointMicro:
                    bx.LoAddress = loadBX(bx, stack, dataSpace);
                    bx.HiAddress = codeSpace[pc + 1];
                    pc += 2;
                    break;
                case pCode.PointBX:
                    index = loadBX(bx, stack, dataSpace);
                    bx.Address = longManager[index];
                    pc++;
                    break;
                case pCode.BFree:
                    if (a0 != 0)
                    {
                        longManager.Free(a0);
                        a0 = 0;
                    }
                    pc++;
                    break;
                case pCode.BFreeA1:
                    if (a1 != 0)
                    {
                        longManager.Free(a1);
                        a1 = 0;
                    }
                    pc++;
                    break;
                case pCode.Iterator:
                    throw new NotImplementedException();
                case pCode.This:
                    a0 = id;
                    pc++;
                    break;
                case pCode.Task:
                    a0 = taskId;
                    pc++;
                    break;
                case pCode.New:
                    index = aliasTable[codeSpace[pc + 1]];
                    a0 = objectManager.New((LiquidClass)index);
                    if (a0 == 0)
                    {
                        task.Raise(ExceptionCode.OutOfMemory);
                        pc += 2;
                        goto Done;
                    }
                    objectManager.Hook(a0, id);
                    pc += 2;
                    break;
                case pCode.Hook:
                    throw new NotImplementedException();
                case pCode.Copy:
                    if (a0 != 0)
                    {
                        a0 = objectManager.Copy(a0);
                    }
                    pc++;
                    break;
                case pCode.Adopt:
                    throw new NotImplementedException();
                case pCode.Assign:
                    data = loadBX(bx, stack, dataSpace);
                    if (data != 0)
                    {
                        objectManager.Mark(data);
                    }
                    saveBX(bx, stack, dataSpace, a0);
                    pc++;
                    break;
                case pCode.Free:
                    if (a0 != 0)
                    {
                        objectManager.Mark(a0);
                        a0 = 0;
                    }
                    pc++;
                    break;
                case pCode.FreeA1:
                    if (a1 != 0)
                    {
                        objectManager.Mark(a1);
                        a1 = 0;
                    }
                    pc++;
                    break;
                case pCode.FreeOnErr:
                    if (a0 != 0)
                    {
                        if (task.ThrowCode != ExceptionCode.None)
                        {
                            objectManager.Mark(a0);
                            a0 = 0;
                            pc++;
                            goto Done;
                        }
                    }
                    pc++;
                    break;
                case pCode.ClassA0:
                    a0 = aliasTable[codeSpace[pc + 1]];
                    pc += 2;
                    break;
                case pCode.ConstA0:
                    a0 = codeSpace[pc + 1];
                    pc += 2;
                    break;
                case pCode.ConstA1:
                    a1 = codeSpace[pc + 1];
                    pc += 2;
                    break;
                case pCode.LoadA0:
                    a0 = loadBX(bx, stack, dataSpace);
                    pc++;
                    break;
                case pCode.LoadA1:
                    a1 = loadBX(bx, stack, dataSpace);
                    pc++;
                    break;
                case pCode.Boolean:
                    if (a0 != 0)
                    {
                        a0 = 1;
                    }
                    pc++;
                    break;
                case pCode.StoreA0:
                    saveBX(bx, stack, dataSpace, a0);
                    pc++;
                    break;
                case pCode.StoreA1:
                    saveBX(bx, stack, dataSpace, a1);
                    pc++;
                    break;
                case pCode.MoveA0A1:
                    a0 = a1;
                    pc++;
                    break;
                case pCode.MoveA1A0:
                    a1 = a0;
                    pc++;
                    break;
                case pCode.PushA0:
                    sp++;
                    if (sp >= sz)
                    {
                        task.Raise(ExceptionCode.StackOverflow);
                        pc++;
                        goto Done;
                    }
                    stack[sp] = a0;
                    pc++;
                    break;
                case pCode.PushA1:
                    sp++;
                    if (sp >= sz)
                    {
                        task.Raise(ExceptionCode.StackOverflow);
                        pc++;
                        goto Done;
                    }
                    stack[sp] = a1;
                    pc++;
                    break;
                case pCode.PopA0:
                    a0 = stack[sp];
                    sp--;
                    pc++;
                    break;
                case pCode.PopA1:
                    a1 = stack[sp];
                    sp--;
                    pc++;
                    break;
                case pCode.MoveA0D0:
                    a0 = (int)d0;
                    pc++;
                    break;
                case pCode.MoveA1D1:
                    a1 = (int)d1;
                    pc++;
                    break;
                case pCode.NotUsed:
                    throw new NotImplementedException();
                case pCode.NegA0:
                    a0 = -a0;
                    pc++;
                    break;
                case pCode.NegA1:
                    a1 = -a1;
                    pc++;
                    break;
                case pCode.NotA0:
                    a0 = ~a0;
                    pc++;
                    break;
                case pCode.NotA1:
                    a1 = ~a1;
                    pc++;
                    break;
                case pCode.IncA0:
                    a0++;
                    pc++;
                    break;
                case pCode.IncA1:
                    a1++;
                    pc++;
                    break;
                case pCode.DecA0:
                    a0--;
                    pc++;
                    break;
                case pCode.DecA1:
                    a1--;
                    pc++;
                    break;
                case pCode.Abs:
                    if (a0 < 0)
                    {
                        a0 = -a0;
                    }
                    pc++;
                    break;
                case pCode.Sgn:
                    if (a0 < 0)
                    {
                        a0 = -1;
                    }
                    else if (a0 > 0)
                    {
                        a0 = 1;
                    }
                    pc++;
                    break;
                case pCode.Add:
                    a0 += a1;
                    pc++;
                    break;
                case pCode.Sub:
                    a0 -= a1;
                    pc++;
                    break;
                case pCode.Mod:
                    a0 %= a1;
                    pc++;
                    break;
                case pCode.Mult:
                    a0 *= a1;
                    pc++;
                    break;
                case pCode.Div:
                    if (a1 == 0)
                    {
                        task.Raise(ExceptionCode.DivisionByZero);
                        pc++;
                        goto Done;
                    }
                    a0 /= a1;
                    pc++;
                    break;
                case pCode.Power:
                    a0 = (int)Math.Pow(a0, a1);
                    pc++;
                    break;
                case pCode.Shl:
                    a0 <<= a1;
                    pc++;
                    break;
                case pCode.Shr:
                    a0 >>= a1;
                    pc++;
                    break;
                case pCode.ShlConst:
                    a0 <<= codeSpace[pc + 1];
                    pc += 2;
                    break;
                case pCode.ShrConst:
                    a0 >>= codeSpace[pc + 1];
                    pc += 2;
                    break;
                case pCode.Equal:
                    a0 = (a0 == a1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.NotEqual:
                    a0 = (a0 != a1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.Less:
                    a0 = (a0 < a1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.LessEqual:
                    a0 = (a0 <= a1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.Greater:
                    a0 = (a0 > a1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.GreaterEqual:
                    a0 = (a0 >= a1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.And:
                    a0 &= a1;
                    pc++;
                    break;
                case pCode.Or:
                    a0 |= a1;
                    pc++;
                    break;
                case pCode.Xor:
                    a0 ^= a1;
                    pc++;
                    break;
                case pCode.LNot:
                    a0 = (a0 != 0) ? 0 : 1;
                    pc++;
                    break;
                case pCode.LAnd:
                    a0 = (a0 != 0 && a1 != 0) ? 1 : 0;
                    pc++;
                    break;
                case pCode.LOr:
                    a0 = (a0 != 0 || a1 != 0) ? 1 : 0;
                    pc++;
                    break;
                case pCode.LXor:
                    a0 = ((a0 == 0 && a1 == 0) || (a0 != 0 && a1 != 0)) ? 1 : 0;
                    pc++;
                    break;
                case pCode.DConstD0Float:
                    d0 = Util.Int2Float(codeSpace[pc + 1]);
                    pc += 2;
                    break;
                case pCode.DConstD1Float:
                    d1 = Util.Int2Float(codeSpace[pc + 1]);
                    pc += 2;
                    break;
                case pCode.DLoadD0Float:
                    d0 = Util.Int2Float(loadBX(bx, stack, dataSpace));
                    pc++;
                    break;
                case pCode.DLoadD0Int:
                    d0 = loadBX(bx, stack, dataSpace);
                    pc++;
                    break;
                case pCode.DLoadD1Float:
                    d1 = Util.Int2Float(loadBX(bx, stack, dataSpace));
                    pc++;
                    break;
                case pCode.DLoadD1Int:
                    d1 = loadBX(bx, stack, dataSpace);
                    pc++;
                    break;
                case pCode.DStoreD0Float:
                    saveBX(bx, stack, dataSpace, Util.Float2Int((float)d0));
                    pc++;
                    break;
                case pCode.DStoreD1Float:
                    saveBX(bx, stack, dataSpace, Util.Float2Int((float)d1));
                    pc++;
                    break;
                case pCode.MoveD0D1:
                    d0 = d1;
                    pc++;
                    break;
                case pCode.MoveD1D0:
                    d1 = d0;
                    pc++;
                    break;
                case pCode.DPushD0Float:
                    sp++;
                    if (sp >= sz)
                    {
                        task.Raise(ExceptionCode.StackOverflow);
                        pc++;
                        goto Done;
                    }
                    stack[sp] = Util.Float2Int((float)d0);
                    pc++;
                    break;
                case pCode.DPushD1Float:
                    sp++;
                    if (sp >= sz)
                    {
                        task.Raise(ExceptionCode.StackOverflow);
                        pc++;
                        goto Done;
                    }
                    stack[sp] = Util.Float2Int((float)d1);
                    pc++;
                    break;
                case pCode.DPopD0Float:
                    d0 = Util.Int2Float(stack[sp]);
                    sp--;
                    pc++;
                    break;
                case pCode.DPopD1Float:
                    d1 = Util.Int2Float(stack[sp]);
                    sp--;
                    pc++;
                    break;
                case pCode.MoveD0A0:
                    d0 = a0;
                    pc++;
                    break;
                case pCode.MoveD1A1:
                    d1 = a1;
                    pc++;
                    break;
                case pCode.DNegD0:
                    d0 = -d0;
                    pc++;
                    break;
                case pCode.DNegD1:
                    d1 = -d1;
                    pc++;
                    break;
                case pCode.DIncD0:
                    d0++;
                    pc++;
                    break;
                case pCode.DIncD1:
                    d1++;
                    pc++;
                    break;
                case pCode.DDecD0:
                    d0--;
                    pc++;
                    break;
                case pCode.DDecD1:
                    d1--;
                    pc++;
                    break;
                case pCode.DInt:
                    d0 = (int)d0;
                    pc++;
                    break;
                case pCode.DAbs:
                    d0 = Math.Abs(d0);
                    pc++;
                    break;
                case pCode.DSgn:
                    d0 = Math.Sign(d0);
                    pc++;
                    break;
                case pCode.DVal:
                    if (!double.TryParse(stringManager[a0], out d0))
                    {
                        task.Raise(ExceptionCode.Denied);
                        pc++;
                        goto Done;
                    }
                    pc++;
                    break;
                case pCode.DFrac:
                    throw new NotImplementedException();
                case pCode.DCeil:
                    d0 = Math.Ceiling(d0);
                    pc++;
                    break;
                case pCode.DFix:
                    throw new NotImplementedException();
                case pCode.DRound:
                    d0 = Math.Round(d0);
                    pc++;
                    break;
                case pCode.DTan:
                    d0 = Math.Tan(d0);
                    pc++;
                    break;
                case pCode.DAtn:
                    d0 = Math.Atan(d0);
                    pc++;
                    break;
                case pCode.DCos:
                    d0 = Math.Cos(d0);
                    pc++;
                    break;
                case pCode.DSin:
                    d0 = Math.Sin(d0);
                    pc++;
                    break;
                case pCode.DExp:
                    d0 = Math.Exp(d0);
                    pc++;
                    break;
                case pCode.DExp2:
                    throw new NotImplementedException();
                case pCode.DExp10:
                    throw new NotImplementedException();
                case pCode.DLog:
                    d0 = Math.Log(d0);
                    pc++;
                    break;
                case pCode.DLog2:
                    throw new NotImplementedException();
                case pCode.DLog10:
                    d0 = Math.Log10(d0);
                    pc++;
                    break;
                case pCode.DSqr:
                    d0 = Math.Sqrt(d0);
                    pc++;
                    break;
                case pCode.DAdd:
                    d0 += d1;
                    pc++;
                    break;
                case pCode.DSub:
                    d0 -= d1;
                    pc++;
                    break;
                case pCode.DMod:
                    d0 %= d1;
                    pc++;
                    break;
                case pCode.DMult:
                    d0 *= d1;
                    pc++;
                    break;
                case pCode.DDiv:
                    if (d1 == 0d)
                    {
                        task.Raise(ExceptionCode.DivisionByZero);
                        pc++;
                        goto Done;
                    }
                    d0 /= d1;
                    pc++;
                    break;
                case pCode.DPower:
                    d0 = Math.Pow(d0, d1);
                    pc++;
                    break;
                case pCode.DEqual:
                    a0 = (d0 == d1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.DNotEqual:
                    a0 = (d0 != d1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.DLess:
                    a0 = (d0 < d1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.DLessEqual:
                    a0 = (d0 <= d1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.DGreater:
                    a0 = (d0 > d1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.DGreaterEqual:
                    a0 = (d0 >= d1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.WConstA0:
                    a0 = stringManager.New(id, stringTable[codeSpace[pc + 1]]);
                    if (a0 == 0)
                    {
                        task.Raise(ExceptionCode.OutOfMemory);
                        pc += 2;
                        goto Done;
                    }
                    pc += 2;
                    break;
                case pCode.WConstA1:
                    a1 = stringManager.New(id, stringTable[codeSpace[pc + 1]]);
                    if (a1 == 0)
                    {
                        task.Raise(ExceptionCode.OutOfMemory);
                        pc += 2;
                        goto Done;
                    }
                    pc += 2;
                    break;
                case pCode.WCloneA0:
                    if (a0 != 0)
                    {
                        index = a0;
                        a0 = stringManager.Clone(index);
                        if (a0 == 0)
                        {
                            task.Raise(ExceptionCode.OutOfMemory);
                            pc++;
                            goto Done;
                        }
                    }
                    pc++;
                    break;
                case pCode.WCloneA1:
                    if (a1 != 0)
                    {
                        index = a1;
                        a1 = stringManager.Clone(index);
                        if (a1 == 0)
                        {
                            task.Raise(ExceptionCode.OutOfMemory);
                            pc++;
                            goto Done;
                        }
                    }
                    pc++;
                    break;
                case pCode.WJoin:
                    if (a1 != 0)
                    {
                        if (a0 == 0)
                        {
                            a0 = stringManager.New(id);
                            if (a0 == 0)
                            {
                                task.Raise(ExceptionCode.OutOfMemory);
                                pc++;
                                goto Done;
                            }
                        }
                        stringManager.Add(a0, stringManager[a1]);
                        pc++;
                    }
                    break;
                case pCode.WStrA0:
                    a0 = stringManager.New(id, Convert.ToString(a0));
                    if (a0 == 0)
                    {
                        task.Raise(ExceptionCode.OutOfMemory);
                        pc++;
                        goto Done;
                    }
                    pc++;
                    break;
                case pCode.WStrD0Float:
                    a0 = stringManager.New(id, Convert.ToString((float)d0));
                    if (a0 == 0)
                    {
                        task.Raise(ExceptionCode.OutOfMemory);
                        pc++;
                        goto Done;
                    }
                    pc++;
                    break;
                case pCode.WHook:
                    throw new NotImplementedException();
                case pCode.WAssign:
                    data = loadBX(bx, stack, dataSpace);
                    if (data != 0)
                    {
                        stringManager.Free(data);
                    }
                    saveBX(bx, stack, dataSpace, a0);
                    pc++;
                    break;
                case pCode.WFree:
                    if (a0 != 0)
                    {
                        stringManager.Free(a0);
                        a0 = 0;
                    }
                    pc++;
                    break;
                case pCode.WFreeA1:
                    if (a1 != 0)
                    {
                        stringManager.Free(a1);
                        a1 = 0;
                    }
                    pc++;
                    break;
                case pCode.WEqual:
                    a0 = (string.Compare(stringManager[a0], stringManager[a1]) == 0) ? 1 : 0;
                    pc++;
                    break;
                case pCode.WNotEqual:
                    a0 = (string.Compare(stringManager[a0], stringManager[a1]) != 0) ? 1 : 0;
                    pc++;
                    break;
                case pCode.WLess:
                    a0 = (string.Compare(stringManager[a0], stringManager[a1]) < 0) ? 1 : 0;
                    pc++;
                    break;
                case pCode.WGreater:
                    a0 = (string.Compare(stringManager[a0], stringManager[a1]) > 0) ? 1 : 0;
                    pc++;
                    break;
                case pCode.Jump:
                    address = codeSpace[pc + 1];
                    pc += 2 + address;
                    break;
                case pCode.JumpFalse:
                    if (a0 == 0)
                    {
                        address = codeSpace[pc + 1];
                        pc += 2 + address;
                    }
                    else
                    {
                        pc += 2;
                    }
                    break;
                case pCode.JumpTrue:
                    if (a0 != 0)
                    {
                        address = codeSpace[pc + 1];
                        pc += 2 + address;
                    }
                    else
                    {
                        pc += 2;
                    }
                    break;
                case pCode.JumpIteratorEnd:
                    throw new NotImplementedException();
                case pCode.Gosub:
                    sp++;
                    if (sp >= sz)
                    {
                        task.Raise(ExceptionCode.StackOverflow);
                        pc += 2;
                        goto Done;
                    }
                    stack[sp] = pc + 2;
                    address = codeSpace[pc + 1];
                    pc += 2 + address;
                    break;
                case pCode.Return:
                    pc = stack[sp];
                    sp--;
                    break;
                case pCode.Native:
                    if (a0 == 0)
                    {
                        task.Raise(ExceptionCode.NullObject);
                        pc += 2;
                        goto Done;
                    }

                    liquidClass = objectManager[a0].LiquidClass;
                    index = aliasTable[codeSpace[pc + 1]];
                    function = Program.ClassManager[liquidClass].Methods[index].Stub;

                    isWaiting = function(liquidClass, a0, stack, sp, ref a0, ref bx, ref c0, ref d0);

                    if (startPC == 0)
                    {
                        if (PCB.State != Liquid.Task.ProcessState.Suspended)
                        {
                            PCB.State = (isWaiting) ? Liquid.Task.ProcessState.Waiting : Liquid.Task.ProcessState.Running;
                        }
                    }

                    if (task.ThrowCode != ExceptionCode.None)
                    {
                        pc += 2;
                        goto Done;
                    }

                    // Don't allow hold methods if not in a task
                    // IF pCode THEN ...
                    // TODO

                    if (isWaiting)
                    {
                        goto Done;
                    }
                    else
                    {
                        pc += 2;
                    }
                    break;
                case pCode.Call:
                    if (a0 == 0)
                    {
                        task.Raise(ExceptionCode.NullObject);
                        pc += 2;
                        goto Done;
                    }

                    if (sp + 4 >= sz)
                    {
                        task.Raise(ExceptionCode.StackOverflow);
                        pc += 2;
                        goto Done;
                    }

                    sp++;
                    stack[sp] = bp;
                    sp++;
                    stack[sp] = id;
                    sp++;
                    stack[sp] = ds;
                    sp++;
                    stack[sp] = pc + 2;

                    bp = sp;
                    id = a0;
                    ds = a0;

                    address = codeSpace[pc + 1];
                    pc += 2 + address;
                    break;
                case pCode.VTable:
                    if (a0 == 0)
                    {
                        task.Raise(ExceptionCode.NullObject);
                        pc += 2;
                        goto Done;
                    }

                    liquidClass = objectManager[a0].LiquidClass;
                    index = aliasTable[codeSpace[pc + 1]];
                    function = Program.ClassManager[liquidClass].Methods[index].Stub;

                    if (function != null)
                    {
                        isWaiting = function(liquidClass, a0, stack, sp, ref a0, ref bx, ref c0, ref d0);

                        if (startPC == 0)
                        {
                            if (PCB.State != Liquid.Task.ProcessState.Suspended)
                            {
                                PCB.State = (isWaiting) ? Liquid.Task.ProcessState.Waiting : Liquid.Task.ProcessState.Running;
                            }
                        }

                        if (task.ThrowCode != ExceptionCode.None)
                        {
                            pc += 2;
                            goto Done;
                        }

                        // Don't allow hold methods if not in a task
                        // IF pCode THEN ...
                        // TODO

                        if (isWaiting)
                        {
                            goto Done;
                        }
                        else
                        {
                            pc += 2;
                        }
                    }
                    else
                    {
                        address = Program.ClassManager[liquidClass].Methods[index].Address;
                        if (address != 0)
                        {
                            if (sp + 4 >= sz)
                            {
                                task.Raise(ExceptionCode.StackOverflow);
                                pc += 2;
                                goto Done;
                            }

                            sp++;
                            stack[sp] = bp;
                            sp++;
                            stack[sp] = id;
                            sp++;
                            stack[sp] = ds;
                            sp++;
                            stack[sp] = pc + 2;

                            bp = sp;
                            id = a0;
                            ds = a0;

                            pc = address;
                        }
                        else
                        {
                            pc += 2;
                        }
                    }
                    break;
                case pCode.NativeClass:
                    if (a0 == 0)
                    {
                        task.Raise(ExceptionCode.NullObject);
                        pc += 3;
                        goto Done;
                    }

                    liquidClass = (LiquidClass)aliasTable[codeSpace[pc + 1]];
                    index = aliasTable[codeSpace[pc + 2]];
                    function = Program.ClassManager[liquidClass].Methods[index].Stub;

                    isWaiting = function(liquidClass, a0, stack, sp, ref a0, ref bx, ref c0, ref d0);

                    if (startPC == 0)
                    {
                        if (PCB.State != Liquid.Task.ProcessState.Suspended)
                        {
                            PCB.State = (isWaiting) ? Liquid.Task.ProcessState.Waiting : Liquid.Task.ProcessState.Running;
                        }
                    }

                    if (task.ThrowCode != ExceptionCode.None)
                    {
                        pc += 3;
                        goto Done;
                    }

                    // Don't allow hold methods if not in a task
                    // IF pCode THEN ...
                    // TODO

                    if (isWaiting)
                    {
                        goto Done;
                    }
                    else
                    {
                        pc += 3;
                    }
                    break;
                case pCode.WaitClass:
                    throw new NotImplementedException();
                case pCode.CallClass:
                    if (a0 == 0)
                    {
                        task.Raise(ExceptionCode.NullObject);
                        pc += 3;
                        goto Done;
                    }

                    if (sp + 4 >= sz)
                    {
                        task.Raise(ExceptionCode.StackOverflow);
                        pc += 3;
                        goto Done;
                    }

                    sp++;
                    stack[sp] = bp;
                    sp++;
                    stack[sp] = id;
                    sp++;
                    stack[sp] = ds;
                    sp++;
                    stack[sp] = pc + 3;

                    bp = sp;
                    id = a0;
                    ds = a0;

                    address = codeSpace[pc + 2];
                    pc += 3 + address;
                    break;
                case pCode.VTableClass:
                    if (a0 == 0)
                    {
                        task.Raise(ExceptionCode.NullObject);
                        pc += 3;
                        goto Done;
                    }

                    liquidClass = (LiquidClass)aliasTable[codeSpace[pc + 1]];
                    index = aliasTable[codeSpace[pc + 2]];
                    function = Program.ClassManager[liquidClass].Methods[index].Stub;

                    if (function != null)
                    {
                        isWaiting = function(liquidClass, a0, stack, sp, ref a0, ref bx, ref c0, ref d0);

                        if (startPC == 0)
                        {
                            if (PCB.State != Liquid.Task.ProcessState.Suspended)
                            {
                                PCB.State = (isWaiting) ? Liquid.Task.ProcessState.Waiting : Liquid.Task.ProcessState.Running;
                            }
                        }

                        if (task.ThrowCode != ExceptionCode.None)
                        {
                            pc += 3;
                            goto Done;
                        }

                        // Don't allow hold functions if not in a task
                        // IF pCode THEN ...
                        // TODO

                        if (isWaiting)
                        {
                            goto Done;
                        }
                        else
                        {
                            pc += 3;
                        }
                    }
                    else
                    {
                        address = Program.ClassManager[liquidClass].Methods[index].Address;
                        if (address != 0)
                        {
                            if (sp + 4 >= sz)
                            {
                                task.Raise(ExceptionCode.StackOverflow);
                                pc += 3;
                                goto Done;
                            }

                            sp++;
                            stack[sp] = bp;
                            sp++;
                            stack[sp] = id;
                            sp++;
                            stack[sp] = ds;
                            sp++;
                            stack[sp] = pc + 3;

                            bp = sp;
                            id = a0;
                            ds = a0;

                            pc = address;
                        }
                        else
                        {
                            pc += 3;
                        }
                    }
                    break;
                case pCode.API:
                    index = aliasTable[codeSpace[pc + 1]];
                    function = Program.API[index].Stub;

                    isWaiting = function(LiquidClass.None, a0, stack, sp, ref a0, ref bx, ref c0, ref d0);

                    if (startPC == 0)
                    {
                        if (PCB.State != Liquid.Task.ProcessState.Suspended)
                        {
                            PCB.State = (isWaiting) ? Liquid.Task.ProcessState.Waiting : Liquid.Task.ProcessState.Running;
                        }
                    }

                    if (task.ThrowCode != ExceptionCode.None)
                    {
                        pc += 2;
                        goto Done;
                    }

                    // Don't allow hold functions if not in a task
                    // IF pCode THEN ...
                    // TODO

                    if (isWaiting)
                    {
                        goto Done;
                    }
                    else
                    {
                        pc += 2;
                    }
                    break;
                case pCode.EndFunc:
                    if (sp == 0 || stack[sp] == 0)
                    {
                        pc = 0;
                        goto Done;
                    }

                    pc = stack[sp];
                    sp--;
                    ds = stack[sp];
                    sp--;
                    id = stack[sp];
                    sp--;
                    bp = stack[sp];
                    sp--;
                    break;
                case pCode.Throw:
                    throw new NotImplementedException();

                case pCode.Byte:
                    a0 = (byte)a0;
                    pc++;
                    break;
                case pCode.ByteA1:
                    a1 = (byte)a1;
                    pc++;
                    break;
                case pCode.Short:
                    a0 = (short)a0;
                    pc++;
                    break;
                case pCode.ShortA1:
                    a1 = (short)a1;
                    pc++;
                    break;
                case pCode.BufferC0:
                    throw new NotImplementedException();
                case pCode.MoveA0C0:
                    a0 = (int)c0;
                    pc++;
                    break;
                case pCode.MoveA1C1:
                    a1 = (int)c1;
                    pc++;
                    break;
                case pCode.MoveC0A0:
                    c0 = a0;
                    pc++;
                    break;
                case pCode.MoveC1A1:
                    c1 = a1;
                    pc++;
                    break;
                case pCode.MoveD0C0:
                    d0 = c0;
                    pc++;
                    break;
                case pCode.MoveD1C1:
                    d1 = c1;
                    pc++;
                    break;
                case pCode.MoveC0D0:
                    c0 = (long)d0;
                    pc++;
                    break;
                case pCode.MoveC1D1:
                    c1 = (long)d1;
                    pc++;
                    break;
                case pCode.MoveC0C1:
                    c0 = c1;
                    pc++;
                    break;
                case pCode.MoveC1C0:
                    c1 = c0;
                    pc++;
                    break;
                case pCode.WStrC0:
                    a0 = stringManager.New(id, Convert.ToString(c0));
                    if (a0 == 0)
                    {
                        task.Raise(ExceptionCode.OutOfMemory);
                        pc++;
                        goto Done;
                    }
                    pc++;
                    break;
                case pCode.QPack:
                    throw new NotImplementedException();
                case pCode.QUnpack:
                    throw new NotImplementedException();
                case pCode.QConstC0:
                    c0 = Util.Int2Long(codeSpace[pc + 1], codeSpace[pc + 2]);
                    pc += 3;
                    break;
                case pCode.QConstC1:
                    c1 = Util.Int2Long(codeSpace[pc + 1], codeSpace[pc + 2]);
                    pc += 3;
                    break;
                case pCode.QLoadC0:
                    c0 = longManager[loadBX(bx, stack, dataSpace)];
                    pc++;
                    break;
                case pCode.QLoadC0Int:
                    c0 = loadBX(bx, stack, dataSpace);
                    pc++;
                    break;
                case pCode.QLoadC1:
                    c1 = longManager[loadBX(bx, stack, dataSpace)];
                    pc++;
                    break;
                case pCode.QLoadC1Int:
                    c1 = loadBX(bx, stack, dataSpace);
                    pc++;
                    break;
                case pCode.QPushC0:
                    sp++;
                    if (sp >= sz)
                    {
                        task.Raise(ExceptionCode.StackOverflow);
                        pc++;
                        goto Done;
                    }
                    index = longManager.New(id, c0);
                    if (index == 0)
                    {
                        task.Raise(ExceptionCode.OutOfMemory);
                        pc++;
                        goto Done;
                    }
                    stack[sp] = index;
                    pc++;
                    break;
                case pCode.QPushC1:
                    sp++;
                    if (sp >= sz)
                    {
                        task.Raise(ExceptionCode.StackOverflow);
                        pc++;
                        goto Done;
                    }
                    index = longManager.New(id, c1);
                    if (index == 0)
                    {
                        task.Raise(ExceptionCode.OutOfMemory);
                        pc++;
                        goto Done;
                    }
                    stack[sp] = index;
                    pc++;
                    break;
                case pCode.QPopC0:
                    index = stack[sp];
                    c0 = longManager[index];
                    longManager.Free(index);
                    sp--;
                    pc++;
                    break;
                case pCode.QPopC1:
                    index = stack[sp];
                    c1 = longManager[index];
                    longManager.Free(index);
                    sp--;
                    pc++;
                    break;
                case pCode.QNegC0:
                    c0 = -c0;
                    pc++;
                    break;
                case pCode.QNegC1:
                    c1 = -c1;
                    pc++;
                    break;
                case pCode.QNotC0:
                    c0 = ~c0;
                    pc++;
                    break;
                case pCode.QNotC1:
                    c1 = ~c1;
                    pc++;
                    break;
                case pCode.QIncC0:
                    c0++;
                    pc++;
                    break;
                case pCode.QIncC1:
                    c1++;
                    pc++;
                    break;
                case pCode.QDecC0:
                    c0--;
                    pc++;
                    break;
                case pCode.QDecC1:
                    c1--;
                    pc++;
                    break;
                case pCode.QAbs:
                    c0 = Math.Abs(c0);
                    pc++;
                    break;
                case pCode.QSgn:
                    c0 = Math.Sign(c0);
                    pc++;
                    break;
                case pCode.QAdd:
                    c0 += c1;
                    pc++;
                    break;
                case pCode.QSub:
                    c0 -= c1;
                    pc++;
                    break;
                case pCode.QMod:
                    c0 %= c1;
                    pc++;
                    break;
                case pCode.QMult:
                    c0 *= c1;
                    pc++;
                    break;
                case pCode.QDiv:
                    if (c1 == 0L)
                    {
                        task.Raise(ExceptionCode.DivisionByZero);
                        pc++;
                        goto Done;
                    }
                    c0 /= c1;
                    pc++;
                    break;
                case pCode.QPower:
                    c0 = (long)Math.Pow(c0, c1);
                    pc++;
                    break;
                case pCode.QShl:
                    c0 = c0 << (int)c1;
                    pc++;
                    break;
                case pCode.QShr:
                    c0 = c0 >> (int)c1;
                    pc++;
                    break;
                case pCode.QShlConst:
                    c0 <<= codeSpace[pc + 1];
                    pc += 2;
                    break;
                case pCode.QShrConst:
                    c0 >>= codeSpace[pc + 1];
                    pc += 2;
                    break;
                case pCode.QEqual:
                    a0 = (c0 == c1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.QNotEqual:
                    a0 = (c0 != c1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.QLess:
                    a0 = (c0 < c1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.QLessEqual:
                    a0 = (c0 <= c1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.QGreater:
                    a0 = (c0 > c1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.QGreaterEqual:
                    a0 = (c0 >= c1) ? 1 : 0;
                    pc++;
                    break;
                case pCode.QAnd:
                    c0 &= c1;
                    pc++;
                    break;
                case pCode.QOr:
                    c0 |= c1;
                    pc++;
                    break;
                case pCode.QXor:
                    c0 ^= c1;
                    pc++;
                    break;
                case pCode.QAssign:
                    data = loadBX(bx, stack, dataSpace);
                    if (data != 0)
                    {
                        longManager.Free(data);
                    }
                    saveBX(bx, stack, dataSpace, longManager.New(id, c0));
                    pc++;
                    break;
                case pCode.QFree:
                    if (a0 != 0)
                    {
                        longManager.Free(a0);
                        a0 = 0;
                    }
                    pc++;
                    break;
                case pCode.QFreeA1:
                    if (a1 != 0)
                    {
                        longManager.Free(a1);
                        a1 = 0;
                    }
                    pc++;
                    break;
                case pCode.QBoolean:
                    if (c0 != 0)
                    {
                        c0 = 1;
                    }
                    pc++;
                    break;
                case pCode.QByte:
                    c0 = (byte)c0;
                    pc++;
                    break;
                case pCode.QShort:
                    c0 = (short)c0;
                    pc++;
                    break;
                case pCode.QInt:
                    c0 = (int)c0;
                    pc++;
                    break;
                case pCode.BufferD0:
                    throw new NotImplementedException();
                case pCode.WStrD0:
                    a0 = stringManager.New(id, Convert.ToString(d0));
                    if (a0 == 0)
                    {
                        task.Raise(ExceptionCode.OutOfMemory);
                        pc++;
                        goto Done;
                    }
                    pc++;
                    break;
                case pCode.DPack:
                    throw new NotImplementedException();
                case pCode.DUnpack:
                    throw new NotImplementedException();
                case pCode.DConstD0:
                    d0 = Util.Int2Double(codeSpace[pc + 1], codeSpace[pc + 2]);
                    pc += 3;
                    break;
                case pCode.DConstD1:
                    d1 = Util.Int2Double(codeSpace[pc + 1], codeSpace[pc + 2]);
                    pc += 3;
                    break;
                case pCode.DLoadD0:
                    d0 = doubleManager[loadBX(bx, stack, dataSpace)];
                    pc++;
                    break;
                case pCode.DLoadD0Int64:
                    d0 = longManager[loadBX(bx, stack, dataSpace)];
                    pc++;
                    break;
                case pCode.DLoadD1:
                    d1 = doubleManager[loadBX(bx, stack, dataSpace)];
                    pc++;
                    break;
                case pCode.DLoadD1Int64:
                    d1 = longManager[loadBX(bx, stack, dataSpace)];
                    pc++;
                    break;
                case pCode.DPushD0:
                    sp++;
                    if (sp >= sz)
                    {
                        task.Raise(ExceptionCode.StackOverflow);
                        pc++;
                        goto Done;
                    }
                    index = doubleManager.New(id, d0);
                    if (index == 0)
                    {
                        task.Raise(ExceptionCode.OutOfMemory);
                        pc++;
                        goto Done;
                    }
                    stack[sp] = index;
                    pc++;
                    break;
                case pCode.DPushD1:
                    sp++;
                    if (sp >= sz)
                    {
                        task.Raise(ExceptionCode.StackOverflow);
                        pc++;
                        goto Done;
                    }
                    index = doubleManager.New(id, d1);
                    if (index == 0)
                    {
                        task.Raise(ExceptionCode.OutOfMemory);
                        pc++;
                        goto Done;
                    }
                    stack[sp] = index;
                    pc++;
                    break;
                case pCode.DPopD0:
                    index = stack[sp];
                    d0 = doubleManager[index];
                    doubleManager.Free(index);
                    sp--;
                    pc++;
                    break;
                case pCode.DPopD1:
                    index = stack[sp];
                    d1 = doubleManager[index];
                    doubleManager.Free(index);
                    sp--;
                    pc++;
                    break;
                case pCode.DAssign:
                    data = loadBX(bx, stack, dataSpace);
                    if (data != 0)
                    {
                        doubleManager.Free(data);
                    }
                    saveBX(bx, stack, dataSpace, doubleManager.New(id, d0));
                    pc++;
                    break;
                case pCode.DFree:
                    if (a0 != 0)
                    {
                        doubleManager.Free(a0);
                        a0 = 0;
                    }
                    pc++;
                    break;
                case pCode.DFreeA1:
                    if (a1 != 0)
                    {
                        doubleManager.Free(a1);
                        a1 = 0;
                    }
                    pc++;
                    break;
                case pCode.XChg:
                    Util.Swap(ref stack[sp], ref stack[sp - 1]);
                    pc++;
                    break;
                default:
                    throw new NotImplementedException();
            }

            goto Continue;

Done:

            if (task.ThrowCode != ExceptionCode.None && !Liquid.Exception.IsFatal(task.ThrowCode))
            {
                if (PCB.IsTrap)
                {
                    task.RaiseException(fn, ln, id);
                    goto Recover;
                }
            }

            if (startPC == 0)
            {
                if (pc == 0)
                {
                    PCB.State = Liquid.Task.ProcessState.Finished;
                }

                PCB.IsTock = true;

                PCB.PC = pc;
                PCB.DS = ds;

                PCB.A0 = a0;
                PCB.A1 = a1;
                PCB.C0 = c0;
                PCB.C1 = c1;
                PCB.D0 = d0;
                PCB.D1 = d1;
                PCB.BX = bx;

                PCB.SP = sp;
                PCB.BP = bp;

                PCB.ID = id;

                PCB.LP = lp;
                PCB.LN = ln;
                PCB.FN = fn;

                PCB.Elapsed = (int)elapsed;
            }

            if (task.ThrowCode != ExceptionCode.None)
            {
                var error = $"Error {(int)task.ThrowCode} in file \"{fileTable[fn]}\" at line {ln}: {task.ThrowCode}";

                if (objectManager[taskId].LiquidClass == LiquidClass.Task)
                {
                    task.ErrorOut(error);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(error);
                }

                PCB.State = Liquid.Task.ProcessState.Crashed;
            }

            stack = null;
            dataSpace = null;
            objectManager = null;

            return a0;
        }
    }
}
