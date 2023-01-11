using System;

namespace MatrixLibrary
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
#pragma warning disable S112 // General exceptions should never be thrown
#pragma warning disable S3249 // Classes directly extending "object" should not call "base" in "GetHashCode" or "Equals"
    public class MatrixException : Exception
    {
        public MatrixException() : base() { }
    }

    public class Matrix : ICloneable
    {
        readonly int rows;
        readonly int columns;
        readonly double[,] matrix;
        public int Rows
        {
            get => rows;
        }

        public int Columns
        {
            get => columns;
        }

        public double[,] Array
        {
            get => matrix;
        }

        public Matrix(int rows, int columns)
        {
            try
            {
                this.rows = rows;
                this.columns = columns;
                matrix = new double[rows, columns];
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        matrix[i, j] = 0;
                    }
                }
            }
            catch
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public Matrix(double[,] array)
        {
            try
            {
                rows = array.GetLength(0);
                columns = array.GetLength(1);
                matrix = array;
            }
            catch
            {
                throw new ArgumentNullException();
            }
        }

        public double this[int row, int column]
        {
            get
            {
                try { return matrix[row, column]; }
                catch { throw new ArgumentException(); }
            }
            set
            {
                try { matrix[row, column] = value; }
                catch { throw new ArgumentException(); }
            }
        }

        public object Clone()
        {
            Matrix m = new Matrix(matrix);
            return m;
        }

        public static Matrix operator +(Matrix matrix1, Matrix matrix2)
        {
            try
            {
                if (matrix1 == null || matrix2 == null)
                    throw new Exception("null");
                if (matrix1.rows != matrix2.rows || matrix1.columns != matrix2.columns)
                    throw new Exception("mx");
                Matrix matrix3 = new Matrix(matrix1.rows, matrix2.columns);
                for (int i = 0; i < matrix3.rows; i++)
                {
                    for (int j = 0; j < matrix3.columns; j++)
                    {
                        matrix3[i, j] = matrix1[i, j] + matrix2[i, j];
                    }
                }
                return matrix3;
            }
            catch (Exception e)
            {
                if (e.Message == "null")
                    throw new ArgumentNullException();
                else if (e.Message == "mx")
                    throw new MatrixException();
                else
                    throw new Exception();
            }
        }

        public static Matrix operator -(Matrix matrix1, Matrix matrix2)
        {
            try
            {
                if (matrix1 == null || matrix2 == null)
                {
                    throw new Exception("null");
                }
                if (matrix1.rows != matrix2.rows || matrix1.columns != matrix2.columns)
                    throw new Exception("mx");
                Matrix matrix3 = new Matrix(matrix1.rows, matrix2.columns);
                for (int i = 0; i < matrix3.rows; i++)
                {
                    for (int j = 0; j < matrix3.columns; j++)
                    {
                        matrix3[i, j] = matrix1[i, j] - matrix2[i, j];
                    }
                }
                return matrix3;
            }
            catch (Exception e)
            {
                if (e.Message == "null")
                    throw new ArgumentNullException();
                else if (e.Message == "mx")
                    throw new MatrixException();
                else
                    throw new Exception();
            }
        }

        public static Matrix operator *(Matrix matrix1, Matrix matrix2)
        {
            try
            {
                if (matrix1 == null || matrix2 == null)
                    throw new Exception("null");
                if (matrix1.columns != matrix2.rows)
                    throw new Exception("mx");
                Matrix matrix3 = new Matrix(matrix1.rows, matrix2.columns);
                for (int i = 0; i < matrix3.rows; i++)
                {
                    for (int j = 0; j < matrix3.columns; j++)
                    {
                        matrix3[i, j] = 0;
                        for (int k = 0; k < matrix1.columns; k++)
                        {
                            matrix3[i, j] += matrix1[i, k] * matrix2[k, j];
                        }
                    }
                }
                return matrix3;
            }
            catch (Exception e)
            {
                if (e.Message == "null")
                    throw new ArgumentNullException();
                else if (e.Message == "mx")
                    throw new MatrixException();
                else
                    throw new Exception();
            }
        }

        public Matrix Add(Matrix matrix)
        {
            try
            {
                if (this.matrix == null || matrix == null)
                {
                    throw new Exception("null");
                }
                if (rows != matrix.rows || columns != matrix.columns)
                    throw new Exception("mx");
                Matrix matrix3 = new Matrix(this.rows, matrix.columns);
                for (int i = 0; i < matrix3.rows; i++)
                {
                    for (int j = 0; j < matrix3.columns; j++)
                    {
                        matrix3[i, j] = this.matrix[i, j] + matrix[i, j];
                    }
                }
                return matrix3;
            }
            catch (Exception e)
            {
                if (e.Message == "null")
                    throw new ArgumentNullException();
                else if (e.Message == "mx")
                    throw new MatrixException();
                else
                    throw new Exception();
            }
        }

        public Matrix Subtract(Matrix matrix)
        {
            try
            {
                if (this.matrix == null || matrix == null)
                    throw new Exception("null");
                if (rows != matrix.rows || columns != matrix.columns)
                    throw new Exception("mx");
                Matrix matrix3 = new Matrix(rows, matrix.columns);
                for (int i = 0; i < matrix3.rows; i++)
                {
                    for (int j = 0; j < matrix3.columns; j++)
                    {
                        matrix3[i, j] = this.matrix[i, j] - matrix[i, j];
                    }
                }
                return matrix3;
            }
            catch (Exception e)
            {
                if (e.Message == "null")
                    throw new ArgumentNullException();
                else if (e.Message == "mx")
                    throw new MatrixException();
                else
                    throw new Exception();
            }
        }

        public Matrix Multiply(Matrix matrix)
        {
            try
            {
                if (columns != matrix.rows)
                {
                    throw new Exception("mx");
                }
                Matrix matrix3 = new Matrix(rows, matrix.columns);
                for (int i = 0; i < matrix3.rows; i++)
                {
                    for (int j = 0; j < matrix3.columns; j++)
                    {
                        matrix3[i, j] = 0;
                        for (int k = 0; k < columns; k++)
                        {
                            matrix3[i, j] += this.matrix[i, k] * matrix[k, j];
                        }
                    }
                }
                return matrix3;
            }
            catch (Exception e)
            {
                if (e.Message == "mx")
                    throw new MatrixException();
                else
                    throw new ArgumentNullException();
            }
        }

        public override bool Equals(object obj)
        {
            try
            {
                Matrix matrix2 = (Matrix)obj;
                if (rows != matrix2.rows || columns != matrix2.columns)
                    return false;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        if (matrix[i, j] != matrix2[i, j])
                            return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
