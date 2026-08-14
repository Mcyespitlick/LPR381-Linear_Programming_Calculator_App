# LPR381-Linear_Programming_Calculator_App
A simple application that reads a input file, with the features of being able to calculate several linear programming models. There are also the features to modify the input data on the fly.
















in-depth explanaitionon how it reads and transforms data is as follows:

it takes the the input in the form of this example:

max +2 +3 +3 +5 +2 +4
+11 +8 +6 +14 +10 +10 <= 40
+8 +5 +12 +3 +6 +10 >= 30
bin urs - bin bin bin

when clicking "to canonical" it then changes the the form into this:
(not, this needs to be done before transforming to standard form, atleast for now)

    x1      x2      x3      x4      x5      x6
max +2      +3      +3      +5      +2      +4
    +11     +8      +6      +14     +10     +10 <= 40
    +8      +5      +12     +3      +6      +10 >= 30
    bin     urs     -       bin     bin     bin


//==================================================================================================================================
//==================================================================================================================================
//==================================================================================================================================



the method "To Canonical" only takes the data and splits it up into a usable form.
It is stored in a Class in the following shape (this is saved in a class instance called "ParsedModel"):


ObjectiveType: "max"    --------------------------------------------------------stored as a string

ObjectiveCoefficients: +2      +3      +3      +5      +2      +4    -----------saved in a Double array (an array that stores Doubles so we can keep the decimals)


//--note, "Constraints" are a class, but its saved in a list

Constraints[0]: Coefficients: 11     8      6      14     10     10   ----------The coefficients are stored in a Double array (Array storing doubles)
                Relation: "<=" -------------------------------------------------Stored as a string
                RHS: 40 --------------------------------------------------------Stored as a double

Constraints[1]: Coenficients: 8      5      12     3      6      10   ----------The coefficients are stored in a Double array (Array storing doubles)
                Relation: ">=" -------------------------------------------------Stored as a string
                RHS: 30 --------------------------------------------------------Stored as a double

SignRestrictions: "bin"     "urs"     "-"       "bin"     "bin"     "bin"   ---saved in a String array    



//==================================================================================================================================
//==================================================================================================================================
//==================================================================================================================================



Clicking "to standard" then changes this canonical form into standard form (the one usually usedin linear programming)
It is sored in a class Called "StandardModel"

Max x1      x2      x2'    x3'    x4      x5      x6      s1     e2     a2  
W   8       5      -5     -12     3       6       10      0      -1     1       30
Z   -2     -3       3      3     -5      -2      -4       0      0      0
1   11      8      -8     -6      14      10      10      1      0      0   =   40
2   8       5      -5     -12     3       6       10      0      -1     1   =   30
   "bin" "none"  "none" "none"  "bin"   "bin"   "bin"  "none" "none" "none"

Its the split further into this shape

ObjectiveType: "max"    --------------------------------------------------------stored as a string

VariableNames:  x1      x2      x2'    x3'    x4      x5      x6      s1     e2     a2 --------------------------saved as a String array

ObjectiveCoefficients: -2      -3      3      3      -5      -2      -4      0      0      0   ------------------saved in a Double List (a list that stores Doubles so we can keep the decimals)



Constraints[0]: Coefficients: 11      8      -8     -6      14      10      10      1      0      0   -----------The coefficients are stored in a Double list (list storing doubles)
                Relation: "<=" -------------------------------------------------Stored as a string
                RHS: 40 --------------------------------------------------------Stored as a double




Constraints[1]: Coefficients: 8       5      -5     -12     3       6       10      0      -1     1   -----------The coefficients are stored in a Double list (list storing doubles)
                Relation: "<=" -------------------------------------------------Stored as a string
                RHS: 40 --------------------------------------------------------Stored as a double



TwoPhaseObjective:  Coefficients: 11      8      -8     -6      14      10      10      1      0      0   -----------The coefficients are stored in a Double list (list storing doubles)
                    Relation: ""----------------------------------------------------its not needed, so its empty
                    RHS: 30 --------------------------------------------------------Stored as a double
