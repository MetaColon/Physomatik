﻿using System;

namespace Physomatik1
{
    class Program
    {
        static void Main(string[] args)
        {
            a:
            double[] start = new double[2] { 0, 45 };
            double[] vector = new double[2] { 10, 45 };
            double t = 0;
            int sizex = Console.LargestWindowWidth, sizey = Console.LargestWindowHeight;
            Console.SetBufferSize(sizex * 10, sizey * 10);
            Console.SetWindowSize(sizex, sizey);
            Console.SetBufferSize(sizex, sizey);
            Console.SetWindowPosition(0, 0);
            Console.CursorVisible = false;
            double[,] simulated = new double[100000, 2];
            int pos = 0;
            Console.SetCursorPosition(0, 0);
            double F = Convert.ToDouble(Console.ReadLine()), a = Convert.ToDouble(Console.ReadLine()), m = 10, f = 0.1, c_w = 1, A = 0.1, s = 10;
            Console.Write("Simulating...");
            double step = 0.001;
            double[] v = new double[2] { 0, a }, posi = new double[2];
            double[,] vectors;
            bool ground = true;
            while (pos < simulated.GetLength(0) - 1)
            {
                try
                {
                    if (Math.Sqrt(posi[0]*posi[0]+posi[1]*posi[1]) < s && ground)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        vectors = Physomatik.getnewPos_SpeedatHill(f, a, m, Physomatik.g, F, step, v, c_w, A, Physomatik.Density_Air, posi);
                    }
                    else if(sizey / 2 - posi[1] < sizey - 10)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        vectors = Physomatik.getnewPos_Speed(new double[2] { 0, 0 }, m, Physomatik.g, c_w, A, Physomatik.Density_Air, step, posi, v);
                        ground = false;
                    }
                    else
                    {
                        vectors = new double[2, 2] { { Physomatik.getnewPos(v, posi, step)[0], Physomatik.getnewPos(v, posi, step)[1] }, { Physomatik.getnewSpeedafterImpact(v, step, c_w, A, Physomatik.Density_Air, m, Physomatik.g, 0.1, f)[0], Physomatik.getnewSpeedafterImpact(v, step, c_w, A, Physomatik.Density_Air, m, Physomatik.g, 0.1, f)[1] } };
                    }
                    v[0] = vectors[1, 0];
                    v[1] = vectors[1, 1];
                    posi[0] = vectors[0, 0];
                    posi[1] = vectors[0, 1];
                    simulated[pos, 0] = posi[0];
                    simulated[pos, 1] = posi[1];
                    Console.SetCursorPosition((int)(sizex / 2 + (posi[0]) / 1), (int)(sizey / 2 - (posi[1]) / 1));
                    Console.Write("█");
                    pos++;
                    t += step;
                }
                catch
                {
                    break;
                }
            }
            pos = 0;
            ground = true;
            Console.Clear();
            while(pos < simulated.GetLength(0)-1)
            {
                try
                {

                    Console.SetCursorPosition((int)(sizex / 2 + (simulated[pos, 0]) / 1), (int)(sizey / 2 - (simulated[pos, 1]) / 1));
                    if (Math.Sqrt(simulated[pos, 0] * simulated[pos, 0] + simulated[pos, 1] * simulated[pos, 1]) < s && ground) Console.ForegroundColor = ConsoleColor.Red;
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        ground = false;
                    }
                    Console.Write("█");
                    System.Threading.Thread.Sleep((int)(step*1000));
                    pos++;
                }
                catch
                {
                    break;
                }
            }
            Console.ReadLine();
            Console.Clear();
            goto a;
        }
    }

    class Physomatik
    {
        public static double PI = 3.14159265359;
        public static double g = 9.81;
        public static double Density_Air = 1.2041;

        #region resVector
        public static double[] getresVector(double[,] vectors)
        {
            double xsum = 0, ysum = 0;
            double[] vector = new double[2];
            for (int i = 0; i < vectors.GetLength(0); i++)
            {
                vector[0] = vectors[i, 0];
                vector[1] = vectors[i, 1];
                xsum += getxPart(vector); //get the parts...
                ysum += getyPart(vector);
            }
            vector[0] = Math.Sqrt(xsum * xsum + ysum * ysum); //and add the parts
            vector[1] = getresAngle(xsum, ysum); //and get the angle
            if (vector[1] < 0) vector[1] = 360 + vector[1];
            vector[1] %= 360;
            return vector;
        }

        public static double getresAngle(double x, double y)
        {
            if (x > 0) //this is ugly code. But I think, theres no better way to do this
                return ToDegree(Math.Atan(y / x));
            if (x == 0)
                return Math.Sign(y) * 90;
            if (y == 0)
                return 180;
            if (y > 0)
                return 180 - ToDegree(Math.Atan(Math.Abs(y) / Math.Abs(x)));
            else
                return 180 + ToDegree(Math.Atan(Math.Abs(y) / Math.Abs(x)));
        }
        #endregion

        #region Shot
        public static double[] getSpeedatShot(double F_Wurf,double angle_Wurf, double m, double g, double c_w, double A, double P, double t, double t_throw, double step) //you should use this part if you forgot your current position/speed
        {
            double F_G = getF_G(m, g), F_L = 0;
            double[] v = new double[2] { 0, 0 }, F_res = new double[2];
            double[,] Fs;
            for (double i = 0; i < t; i+=step)
            {
                F_L = getF_L(c_w, A, P, v[0]);
                if (i <= t_throw)
                    Fs = new double[3, 2] { { F_G, 270 }, { F_Wurf, angle_Wurf }, { F_L, (v[1] + 180)%360 } };
                else
                    Fs = new double[2, 2] { { F_G, 270 }, { F_L, (v[1] + 180)%360 } };
                F_res = getresVector(Fs);
                v = getnewSpeed(v, F_res, step, m);
            }
            return v;
        }

        public static double[] getPosatShot(double F_Wurf, double angle_Wurf, double m, double g, double c_w, double A, double P, double t, double t_throw, double step)
        {
            double[] pos = new double[2], speed = new double[2];
            for (double i = 0; i < t; i += step)
            {
                speed = getSpeedatShot(F_Wurf, angle_Wurf, m, g, c_w, A, P, i, t_throw, step);
                pos[0] += getxPart(speed) * step;
                pos[1] += getyPart(speed) * step;
            }
            return pos;
        } //as well as this

        public static double[,] getnewPos_Speed(double[] F_Wurf, double m, double g, double c_w, double A, double P, double step, double[] oldpos, double[] oldv) //this gives you a new position/speed - it's stepwise because air is bad
        {
            double[,] vectors = new double[3, 2] { { F_Wurf[0], F_Wurf[1] }, { getF_G(m, g), 270 }, { getF_L(c_w, A, P, oldv[0]), (oldv[1] + 180) % 360 } };
            double[] newSpeed = getnewSpeed(oldv, getresVector(vectors), step, m);
            double[] pos = new double[2] { getxPart(newSpeed) * step + oldpos[0], getyPart(newSpeed) * step + oldpos[1] };
            return new double[2,2] { { pos[0], pos[1] },{ newSpeed[0], newSpeed[1] }};
        }

        public static double[] getPosatShotwithS(double F_Wurf, double angle_Wurf, double m, double g, double c_w, double A, double P, double t, double s_throw, double step)
        {
            double[] pos = new double[2], speed = new double[2];
            double t_throw = t;
            for (double i = 0; i < t; i += step)
            {
                if (Math.Sqrt(pos[0] * pos[0] + pos[1] * pos[1]) > s_throw && t_throw == t) t_throw = i;
                speed = getSpeedatShot(F_Wurf, angle_Wurf, m, g, c_w, A, P, i, t_throw, step);
                pos[0] += getxPart(speed) * step;
                pos[1] += getyPart(speed) * step;
            }
            return pos;
        } //like this one

        public static double getSpeedwithAir(double A, double F, double P, double c_w, double t, double m) //this formula only works with positive or not too negative F
        {
            return ((Math.Sqrt(2 * c_w * P * t * t * A * F + m * m) - m) / c_w * P * t * A);
        }

        public static double getSpeedwithAirandStart(double A, double F, double P, double c_w, double t, double v0, double m) //...just like this
        {
            return ((Math.Sqrt(2 * A * F * P * c_w * t * t + 2 * A * P * c_w * v0 * m * t + m * m) - m) / c_w * P * t * A);
        }
        #endregion

        public static double getDeltaSpeed(double a, double dt)
        {
            return a * dt;
        } //this one is obvious

        public static double[] getnewSpeedafterImpact(double[] v0,  double step, double c_w, double A, double P, double m, double g, double t, double f)
        {
            double vx = getxPart(v0), vy = getyPart(v0);
            double F_N = getF_G(m, g) + getF(vy, t, m);
            if(vx > 0)
                vx += geta(-f * F_N - getF_L(c_w, A, P, vx), m) * step;
            else
                vx += geta(+f * F_N + getF_L(c_w, A, P, vx), m) * step;
            return getresVector(new double[2,2] { { vx, 0 }, { 0, 90 } });
        }

        public static double[] getnewPos(double[] v, double[] pos, double step)
        {
            return new double[2] { pos[0] + getxPart(v) * step, pos[1] + getyPart(v) * step };
        }

        #region getF
        public static double getF_G(double m, double g) //return the fitting Fs
        {
            return m * g;
        }

        public static double getF_L(double c_w, double A, double P, double v)
        {
            return 0.5 * c_w * A * P * v * v;
        }

        public static double getF_N(double angle, double m, double g)
        {
            return Math.Cos(ToRadian(angle)) * getF_G(m, g);
        }

        public static double getF_H(double angle, double m, double g)
        {
            return Math.Sin(ToRadian(angle)) * getF_G(m, g);
        }

        public static double getF_R(double f, double angle, double m, double g)
        {
            return f * getF_N(angle, m, g);
        }
        #endregion

        #region Hill
        public static double getnewSpeedatHill(double f, double angle, double m, double g, double F_S, double step, double[] v0, double c_w, double A, double P) //it's nearly the same as the shot - you just can't fall down and have more resistance
        {
            double v = v0[0];
            if (v0[1] > (angle + 180) % 360-5 && v0[1] < (angle + 180) % 360+5)
                v *= -1;
            if(v > 0)
                return v + ((F_S - getF_H(angle, m, g) - getF_R(f, angle, m, g) - getF_L(c_w, A, P, v)) / m) * step; 
            else
                return v + ((F_S - getF_H(angle, m, g) + getF_R(f, angle, m, g) + getF_L(c_w, A, P, v)) / m) * step;
        }

        public static double[] getnewPosatHill(double f, double angle, double m, double g, double F_S, double step, double[] v0, double c_w, double A, double P, double[] pos0)
        {
            double[] newpos = new double[2] { pos0[0], pos0[1] };
            double newspeed = getnewSpeedatHill(f, angle, m, g, F_S, step, v0, c_w, A, P);
            newpos[0] += getxPart(new double[] { newspeed*step, angle });
            newpos[1] += getyPart(new double[] { newspeed*step, angle });
            return newpos;
        }

        public static double[,] getnewPos_SpeedatHill(double f, double angle, double m, double g, double F_S, double step, double[] v0, double c_w, double A, double P, double[] pos0)
        {
            double[] newSpeed = new double[2] { getnewSpeedatHill(f, angle, m, g, F_S, step, v0, c_w, A, P), angle };
            if (newSpeed[0] < 0)
            {
                newSpeed[0] *= -1;
                newSpeed[1] = (newSpeed[1] + 180) % 360;
            }
            return new double[2, 2] { { getnewPosatHill(f, angle, m, g, F_S, step, v0, c_w, A, P, pos0)[0], getnewPosatHill(f, angle, m, g, F_S, step, v0, c_w, A, P, pos0)[1] },{ newSpeed[0], newSpeed[1] }};
        }
        #endregion

        #region newSpeed
        public static double[] getnewSpeed(double[] vectorv, double[] F, double dt, double m) //returns the new Speed in relativity to the old one
        {
            double[,] vectors = new double[2, 2] { { getnewSpeed(getxPart(vectorv), getxPart(F), dt, m), 0 }, { getnewSpeed(getyPart(vectorv), getyPart(F), dt, m), 90 } };
            return getresVector(vectors);
        }

        public static double getnewSpeed(double v0, double F, double dt, double m)
        {
            return v0 + getDeltaSpeed(geta(F, m), dt);
        }

        public static double getnewSpeed(double v0, double a, double dt)
        {
            return v0 + getDeltaSpeed(a, dt);
        }

        #endregion

        #region FAM
        public static double getF(double v, double t, double m)
        {
            return (v / t) * m;
        }

        public static double geta(double F, double m)
        {
            return F / m;
        }
        #endregion

        #region getPart
        public static double getyPart(double[] vector) //returns the y-/x-Part
        {
            return Math.Sin(ToRadian(vector[1])) * vector[0];
        }

        public static double getxPart(double[] vector)
        {
            return Math.Cos(ToRadian(vector[1])) * vector[0];
        }
        #endregion

        #region ToRadian/Degree
        public static double ToRadian(double degree) //why does C# use radians?
        {
            return degree * PI / 180;
        }

        public static double ToDegree(double radian)
        {
            return radian * 180 / PI;
        }
        #endregion
    }
}
