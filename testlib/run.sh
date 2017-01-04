#!/bin/bash
cd ..
echo "#################"
echo "#   COMPILING   #"
echo "#################"
dotnet restore
dotnet run -p ./src/Lahda.Builder ./testlib/out.s ./testlib/$1
echo "#################"
echo "#   EXECUTING   #"
echo "#################"
./MSM/msm ./testlib/out.s