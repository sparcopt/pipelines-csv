## Overview

Experimenting with `System.IO.Pipelines` to efficiently parse large CSV files, while also evaluating its performance against other parsing techniques. Implementation is based on [this article](https://www.codemag.com/article/2403091).  
Added a few more approaches and benchmarks for each one.

## Benchmarks

- Reading `.csv` file with 20,000,000 records of user sales data
- Aggregatting fields (quantity, price) by user

```
BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3155/23H2/2023Update/SunValley3)  
11th Gen Intel Core i7-11800H 2.30GHz, 1 CPU, 16 logical and 8 physical cores  
.NET SDK 8.0.203  
[Host]     : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI  
DefaultJob : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
```


| Method               | Mean     | Error    | StdDev   | Ratio        | Gen0        | Gen1        | Gen2       | Allocated   | Alloc Ratio |
|--------------------- |---------:|---------:|---------:|-------------:|------------:|------------:|-----------:|------------:|------------:|
| Stream_Parse         | 10.288 s | 0.0646 s | 0.0604 s |     baseline | 878000.0000 | 225000.0000 | 10000.0000 | 10569.71 MB |             |
| Stream_ParseAsMemory |  8.968 s | 0.0411 s | 0.0364 s | 1.15x faster | 381000.0000 |  84000.0000 |  9000.0000 |  4615.78 MB |  2.29x less |
| Stream_ParseAsSpan   |  6.752 s | 0.0272 s | 0.0255 s | 1.52x faster | 367000.0000 |   5000.0000 |  3000.0000 |  4593.25 MB |  2.30x less |
| Pipeline_Utf8Parse   |  4.454 s | 0.0202 s | 0.0179 s | 2.31x faster |   3000.0000 |   3000.0000 |  3000.0000 |   242.63 MB | 43.56x less |

## Running the benchmarks

1. Run script to generate `.csv` file  
   ```bash
   python gen_data.py
   ```
2. Change file path in the benchmark file to match the path of the generated file
3. Browse to the project directory and execute
   ```
   dotnet run -c:Release
   ```
