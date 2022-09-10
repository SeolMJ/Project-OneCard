using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SeolMJ
{
    public static class Utils
    {

        #region Vectors
        public static float Vec2Deg(Vector2 vector)
            => Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

        public static bool IsZero(this Vector3 vector)
            => vector.x == 0 && vector.y == 0 && vector.z == 0;

        public static bool IsZero(this Vector2 vector)
            => vector.x == 0 && vector.y == 0;

        public static bool IsZero(this Vector2Int vector)
            => vector.x == 0 && vector.y == 0;

        public static bool IsApproximatelyZero(this Vector2 vector) =>
            Mathf.Approximately(vector.x, 0) && Mathf.Approximately(vector.y, 0);

        public static bool IsAlmostZero(this Vector2 vector, float limit) =>
            Mathf.Abs(vector.x) < limit && Mathf.Abs(vector.y) < limit;

        public static float AbsDeltaAngle(float one, float two)
            => Mathf.Abs(Mathf.DeltaAngle(one, two));

        public static bool IsInt(this Vector2 vector)
            => vector == Vector2Int.RoundToInt(vector);

        public static Vector2 Clamp(this Vector2 vector)
            => vector.normalized * Mathf.Clamp(vector.magnitude, 0, 1);

        public static Vector2 NemoNemoBeam(this Vector2 vector)
            => vector * Mathf.Min(Mathf.Abs(1f / vector.x), Mathf.Abs(1f / vector.y));

        public static Vector2 Sign(this Vector2 vector)
            => new Vector2(Mathf.Sign(vector.x), Mathf.Sign(vector.y));

        public static Vector2Int SignToInt(this Vector2 vector)
            => new Vector2Int(Math.Sign(vector.x), Math.Sign(vector.y));

        public static Vector2 SpringDamp(Vector2 current, Vector2 target, ref Vector2 velocity, ref Vector2 velvel, float speed, float damp)
            => SpringDamp(current, target, ref velocity, ref velvel, speed, damp, Time.deltaTime);

        public static Vector2 SpringDamp(Vector2 current, Vector2 target, ref Vector2 velocity, ref Vector2 velvel, float speed, float damp, float deltaTime)
        {
            velocity += speed * deltaTime * (target - current);
            velocity = Vector2.SmoothDamp(velocity, Vector2.zero, ref velvel, damp, Mathf.Infinity, deltaTime);
            return current + velocity * deltaTime;
        }

        public static Vector2 SpringDampLimited(Vector2 current, Vector2 target, ref Vector2 velocity, ref Vector2 velvel, float speed, float damp, float limit)
            => SpringDampLimited(current, target, ref velocity, ref velvel, speed, damp, limit, Time.deltaTime);

        public static Vector2 SpringDampLimited(Vector2 current, Vector2 target, ref Vector2 velocity, ref Vector2 velvel, float speed, float damp, float limit, float deltaTime)
        {
            velocity += speed * deltaTime * (target - current);
            velocity = Vector2.SmoothDamp(velocity, Vector2.zero, ref velvel, damp, Mathf.Infinity, deltaTime);
            velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, 0, limit);
            return current + velocity * deltaTime;
        }

        public static Vector3 ToVector3(float scale) =>
            new Vector3(scale, scale, scale);

        public static Vector3 ToVector3(float scale, float z) =>
            new Vector3(scale, scale, z);

        public static Vector3 ToVector3(Vector2 vector, float z) => 
            new Vector3(vector.x, vector.y, z);
        #endregion

        #region Ints

        public static int SignToInt(float input)
        {
            if (input > 0) return 1;
            if (input < 0) return -1;
            return 0;
        }

        #endregion

        #region Floats
        public static float SpringDamp(float current, float target, ref float velocity, ref float velvel, float speed, float damp)
            => SpringDamp(current, target, ref velocity, ref velvel, speed, damp, Time.deltaTime);

        public static float SpringDamp(float current, float target, ref float velocity, ref float velvel, float speed, float damp, float deltaTime)
        {
            velocity += speed * deltaTime * (target - current);
            velocity = Mathf.SmoothDamp(velocity, 0, ref velvel, damp, Mathf.Infinity, deltaTime);
            return current + velocity * deltaTime;
        }

        public static float SpringDamp(float current, float target, ref float velocity, ref float velvel, float speed, float damp, float deltaTime, float limit)
        {
            velocity += speed * deltaTime * (target - current);
            velocity = Mathf.SmoothDamp(velocity, 0, ref velvel, damp, Mathf.Infinity, deltaTime);
            velocity = Mathf.Clamp(velocity, -limit, limit);
            return current + velocity * deltaTime;
        }
        #endregion

        #region Colors
        public static void EditAlpha(this Image image, float alpha)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }

        public static Color OnlyAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
        #endregion

        #region Threading
        public static Task waitOneMiliSec = Task.Delay(1);

        public static async void TimeOut(Action action, TimeSpan timeout, Action onTimeOut = null, Action onSuccess = null)
        {
            Task task = Task.Run(action);
            using (var cancel = new CancellationTokenSource())
            {
                Task completed = await Task.WhenAny(task, Task.Delay(timeout, cancel.Token));
                if (completed == task)
                {
                    if (completed.IsFaulted) throw completed.Exception;
                    cancel.Cancel();
                    onSuccess?.Invoke();
                }
                else
                {
                    onTimeOut?.Invoke();
                }
            }
        }
        #endregion

        #region SmoothDamp
        public static void SmoothDamp(ref this Vector3 current, Vector3 to, ref Vector3 velocity, float time)
        {
            current = Vector3.SmoothDamp(current, to, ref velocity, time);
        }

        public static void SmoothDamp(ref this Vector2 current, Vector2 to, ref Vector2 velocity, float time)
        {
            current = Vector2.SmoothDamp(current, to, ref velocity, time);
        }

        public static void SmoothDamp(this RectTransform rect, Vector2 target, ref Vector2 velocity, float speed, float deltaTime, float maxSpeed = float.PositiveInfinity)
        {
            rect.anchoredPosition = Vector2.SmoothDamp(rect.anchoredPosition, target, ref velocity, speed, maxSpeed, deltaTime);
        }

        public static void SmoothDamp(ref this float current, float target, ref float velocity, float time)
        {
            current = Mathf.SmoothDamp(current, target, ref velocity, time);
        }
        #endregion

        #region Components
        public static T Copy<T>(this Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }

        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().Copy(toAdd);
        }
        #endregion

        #region GameObjects
        public static void DeCloneName(GameObject thisObject)
        {
            if (thisObject.name.Contains("(Clone)")) thisObject.name = thisObject.name.Replace("(Clone)", "");
        }
        #endregion

        #region Coroutines
        private static WaitForEndOfFrame waitNextFrame;
        public static WaitForEndOfFrame WaitNextFrame()
        {
            if (waitNextFrame == null) waitNextFrame = new WaitForEndOfFrame();
            return waitNextFrame;
        }
        #endregion

        #region Array
        public static void Shuffle<T>(this T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = UnityEngine.Random.Range(0, n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = UnityEngine.Random.Range(0, n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }

        public static void Shuffle<T, D>(this Dictionary<T, D> dict)
        {
            System.Random rand = new();
            dict = dict.OrderBy(x => rand.Next())
              .ToDictionary(item => item.Key, item => item.Value);

        }

        public static bool Contains<T>(this T[] array, T item)
        {
            int count = array.Length;
            for (int i = 0; i < count; i++)
            {
                if (array[i].Equals(item)) return true;
            }
            return false;
        }
        #endregion

        #region Compute

        public static async Task<long> ComputeAsync(string exp)
        {
            return await Task.Run(() => Compute(ref exp));
        }

        public static long Compute(ref string exp)
        {
            bool started = false;
            long result = 0;
            long numTemp = 0;
            List<ComputeParen> parenStack = new();
            short opTemp = 0;
            bool prevNum = false;
            for (int i = 0; i < exp.Length; i++)
            {
                char c = exp[i];

                if ('0' <= c && c <= '9')
                {
                    numTemp *= 10;
                    numTemp += (c - '0');
                    prevNum = true;
                    continue;
                }
                else if (prevNum)
                {
                    if (opTemp != 0)
                    {
                        if (parenStack.Count == 0)
                        {
                            switch (opTemp)
                            {
                                case 1: result += numTemp; break;
                                case -1: result -= numTemp; break;
                                case 2: result *= numTemp; break;
                                case -2: result *= -numTemp; break;
                                case 3: result /= numTemp; break;
                                case -3: result /= -numTemp; break;
                            }
                        }
                        else
                        {
                            switch (opTemp)
                            {
                                case 1: parenStack[^1] = new(parenStack[^1].op, parenStack[^1].num + numTemp, true); break;
                                case -1: parenStack[^1] = new(parenStack[^1].op, parenStack[^1].num - numTemp, true); break;
                                case 2: parenStack[^1] = new(parenStack[^1].op, parenStack[^1].num * numTemp, true); break;
                                case -2: parenStack[^1] = new(parenStack[^1].op, parenStack[^1].num * -numTemp, true); break;
                                case 3: parenStack[^1] = new(parenStack[^1].op, parenStack[^1].num / numTemp, true); break;
                                case -3: parenStack[^1] = new(parenStack[^1].op, parenStack[^1].num / -numTemp, true); break;
                            }
                        }
                    }
                    else if (parenStack.Count > 0)
                    {
                        if (parenStack[^1].started) throw new InvalidOperationException("Invalid Expression"); // no operator between numbers
                        parenStack[^1] = new(parenStack[^1].op, numTemp, true);
                    }
                    else
                    {
                        if (started) throw new InvalidOperationException("Invalid Expression"); // no operator between numbers
                        result += numTemp;
                        started = true;
                    }
                    numTemp = 0;
                    opTemp = 0;
                    prevNum = false;
                }

                if (c == '(')
                {
                    parenStack.Add(new(opTemp));
                    opTemp = 0;
                    continue;
                }

                if (c == ')')
                {
                    if (opTemp != 0) throw new InvalidOperationException("Unfinished Expression"); // no numbers after operator
                    if (parenStack.Count == 1)
                    {
                        switch (parenStack[^1].op)
                        {
                            case 0:
                                if (started) throw new InvalidOperationException("Invalid Expression"); // no operator between numbers and paren
                                result = parenStack[^1].num;
                                break;
                            case 1: result += parenStack[^1].num; break;
                            case -1: result -= parenStack[^1].num; break;
                            case 2: result *= parenStack[^1].num; break;
                            case -2: result *= -parenStack[^1].num; break;
                            case 3: result /= parenStack[^1].num; break;
                            case -3: result /= -parenStack[^1].num; break;
                        }
                    }
                    else if (parenStack.Count > 1)
                    {
                        switch (parenStack[^1].op)
                        {
                            case 0:
                                if (parenStack[^2].started) throw new InvalidOperationException("Invalid Expression"); // no operator between numbers and paren
                                parenStack[^2] = new(parenStack[^2].op, parenStack[^1].num, true);
                                break;
                            case 1: parenStack[^2] = new(parenStack[^2].op, parenStack[^2].num + parenStack[^1].num, true); break;
                            case -1: parenStack[^2] = new(parenStack[^2].op, parenStack[^2].num - parenStack[^1].num, true); break;
                            case 2: parenStack[^2] = new(parenStack[^2].op, parenStack[^2].num * parenStack[^1].num, true); break;
                            case -2: parenStack[^2] = new(parenStack[^2].op, parenStack[^2].num * -parenStack[^1].num, true); break;
                            case 3: parenStack[^2] = new(parenStack[^2].op, parenStack[^2].num / parenStack[^1].num, true); break;
                            case -3: parenStack[^2] = new(parenStack[^2].op, parenStack[^2].num / -parenStack[^1].num, true); break;
                        }
                    }
                    else throw new InvalidOperationException("Invalid Expression"); // ')' operator before placing '('
                    parenStack.RemoveAt(parenStack.Count - 1);
                    opTemp = 0;
                    continue;
                }

                if (c == '+')
                {
                    if (opTemp == 0)
                    {
                        opTemp = 1;
                    }
                    continue;
                }

                if (c == '-')
                {
                    if (opTemp == 0)
                    {
                        opTemp = -1;
                        continue;
                    }
                    opTemp *= -1;
                    continue;
                }

                if (c == '*')
                {
                    if (opTemp == 1 || opTemp == -1) throw new InvalidOperationException("Invalid Expression"); // '*' operator after '+' or '-'
                    opTemp = 2;
                    continue;
                }

                if (c == '/')
                {
                    if (opTemp == 1 || opTemp == -1) throw new InvalidOperationException("Invalid Expression"); // '/' operator after '+' or '-'
                    opTemp = 3;
                    continue;
                }
            }

            if (parenStack.Count != 0) throw new InvalidOperationException("Unfinished Expression"); // paren not ended

            if (prevNum)
            {
                switch (opTemp)
                {
                    case 0:
                        if (started) throw new InvalidOperationException("Invalid Expression");  // no operator between numbers
                        else result += numTemp;
                        break;
                    case 1: result += numTemp; break;
                    case -1: result -= numTemp; break;
                    case 2: result *= numTemp; break;
                    case -2: result *= -numTemp; break;
                    case 3: result /= numTemp; break;
                    case -3: result /= -numTemp; break;
                }
            }
            else if (opTemp != 0) throw new InvalidOperationException("Unfinished Expression"); // no numbers after operator

            return result;
        }

        #region Lukince's Method
        /*
        public static async Task<long> Execute(string exp)
        {
            return await Task.Run(() =>
            {
                List<char> list = new();

                if (!GetPostFix(in exp, ref list)) throw new InvalidOperationException("Invalid Expression");

                int tmp = 0;
                Stack<long> stack = new();

                foreach (char c in list)
                {
                    if ('0' <= c && c <= '9' || c < 0)
                    {
                        if (tmp == 0 && c == '0') throw new InvalidOperationException("Starting With '0' is Not Allowed");
                        tmp *= 10;
                        if (c > 0) tmp += c - '0';
                        else tmp -= Mathf.Abs(c) - '0';
                        continue;
                    }

                    if (tmp != 0)
                    {
                        stack.Push(tmp);
                        tmp = 0;
                    }

                    if (c == ' ') continue;

                    if (stack.Count() < 2)
                    {
                        throw new InvalidOperationException("Unfinished Expression");
                    }

                    long a = stack.Pop(), b = stack.Pop();

                    switch (c)
                    {
                        case '+': stack.Push(b + a); break;
                        case '-': stack.Push(b - a); break;
                        case '*': stack.Push(b * a); break;
                        case '/': stack.Push(b / a); break;
                        default: throw new InvalidOperationException($"Invalid Operator: '{c}'");
                    }
                }

                if (stack.Count() == 0) return long.Parse(exp);

                return stack.Peek();
            });
        }

        public static bool GetPostFix(in string exp, ref List<char> list)
        {
            Stack<char> stack = new();
            int numc = 0, opc, i = 0;
            bool flag = false;

            if (exp[0] == '+') i = 1;
            else if (exp[0] == '-') { i = 1; flag = true; }
            else if (('0' > exp[0] || exp[0] > '9') && exp[0] != '(') return false;

            if (('0' > exp[0] || exp[0] > '9') && ('0' > exp[1] || exp[1] > '9'))
            {
                int res = 1;
                int j = 0;
                for (; ; j++)
                {
                    if (exp[j] == '-') res *= -1;
                    else if (exp[j] != '+') break;
                }
                if (j > 1)
                {
                    exp.Remove(0, j);
                    exp.Insert(0, res == 1 ? "+" : "-");
                }
                else return false;
            }

            for (int size = exp.Length; i < size; i++)
            {
                char c = exp[i];
                if ('0' <= c && c <= '9')
                {
                    if (numc > 15) return false;

                    numc++;
                    if (flag)
                    {
                        list.Add((char)-(long)char.GetNumericValue(c));
                        flag = false;
                    }
                    else list.Add(c);
                    continue;
                }

                list.Add(' ');
                if (i > 0 && (exp[i - 1] == '+' || exp[i - 1] == '-'))
                {
                    if (i > 1 && ('0' > exp[i - 2] || exp[i - 2] > '9'))
                    {
                        int res = 1;
                        int j = -1;
                        for (; ; j++)
                        {
                            if (exp[j] == '-') res *= -1;
                            else if (exp[j] != '+') break;
                        }
                        if (j > 0)
                        {
                            exp.Remove(i - 1, j);
                            exp.Insert(i - 1, res == 1 ? "+" : "-");
                        }
                        else return false;
                    }
                    flag = true;
                    continue;
                }
                numc = 0;

                if (c == ')')
                {
                    opc = 0;

                    if (stack.Count() == 0) return false;

                    while (stack.Peek() != '(')
                    {
                        if ('0' > stack.Peek() || stack.Peek() > '9') opc++;
                        list.Add(stack.Pop());
                    }

                    if (opc == 0) return false;

                    stack.Pop();
                    continue;
                }

                while (stack.Count != 0 && GetPriority(c) <= GetPriority(stack.Peek(), true)) list.Add(stack.Pop());

                stack.Push(c);
            }

            while (stack.Count != 0) list.Add(stack.Pop());

            return true;
        }

        public static int GetPriority(char token, bool stack = false) =>
            token switch
            {
                '(' => stack ? 0 : 3,
                '*' => 2,
                '/' => 2,
                '+' => 1,
                '-' => 1,
                _ => -1
            };
        */
        #endregion

        public struct ComputeParen
        {
            public long num;
            public short op;
            public bool started;

            public ComputeParen(short op, long num = 0, bool started = false)
            {
                this.num = num;
                this.op = op;
                this.started = started;
            }

        }

        #endregion

    }

    #region Enums

    public enum Direction
    {
        Up, Down, Left, Right
    }

    #endregion

    #region Structs

    #endregion

    #region Else

    public class CountDictionary<TKey> : IDictionary<TKey, int> where TKey : notnull // by Lukince
    {
        public CountDictionary()
        {
            Collections = new();
        }
        public CountDictionary(IEnumerable<KeyValuePair<TKey, int>> pair)
        {
            Collections = new(pair);
        }
        private Dictionary<TKey, int> Collections;
        public int this[TKey key] { get => Collections[key]; set => Collections[key] = value; }
        public ICollection<TKey> Keys => Collections.Keys;
        public ICollection<int> Values => Collections.Values;
        public int Count => Keys.Count;
        public bool IsReadOnly => false;
        public void Add(TKey key, int value)
        {
            if (Collections.ContainsKey(key))
                Collections[key] += value;
            else
                Collections.Add(key, value);
        }
        public void Add(TKey key)
            => Add(key, 1);
        public void Minus(TKey key, int value)
        {
            if (Collections.ContainsKey(key))
                Collections[key] -= value;
            else
                throw new KeyNotFoundException();
        }
        public void Minus(TKey key)
            => Minus(key, 1);
        public void Add(KeyValuePair<TKey, int> item)
            => Add(item.Key, item.Value);
        public void Clear()
            => Collections.Clear();
        public bool Contains(KeyValuePair<TKey, int> item)
            => Collections.Contains(item);
        public bool ContainsKey(TKey key)
            => Collections.ContainsKey(key);
        public void CopyTo(KeyValuePair<TKey, int>[] array, int arrayIndex)
            => new NotImplementedException();
        public IEnumerator<KeyValuePair<TKey, int>> GetEnumerator()
            => Collections.GetEnumerator();
        public bool Remove(TKey key)
            => Collections.Remove(key);
        public bool Remove(KeyValuePair<TKey, int> item)
            => Collections.Remove(item.Key);
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out int value)
        {
            if (Collections.TryGetValue(key, out int values))
            {
                value = values;
                return true;
            }
            value = default;
            return false;
        }
        IEnumerator IEnumerable.GetEnumerator()
            => Collections.GetEnumerator();
    }

    #endregion

}