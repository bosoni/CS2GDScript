CS2GDScript
(c) mjt, 2019
Released under MIT-license.

Program tries to convert c# code to gdscript.

It is just a helper program and you will need to edit exported .gd script.

Notes:
* program does not convert 'for' loop, edit exported .gd file.

* converter converts function names ie  MyFunctionCall() to my_function_call()
* if there is multiple function calls at the same line, it does not work right
(because converter converts only first one, known bug) 
ie
    if(GetEnergy()==0 && GetEnemyEnergy()==something)
 converts to
   if(get_energy()==0 && GetEnemyEnergy()==something):
   
so add function names with //@f MyFunctionName
so program converts it to my_function_name
