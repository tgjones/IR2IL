#include <stdlib.h>
#include <stdio.h>
#include <math.h>

#include <string.h>
int main()
{
	int  a[10] ;
	int  i ;
	for(i=0 ; i<10 ; i++)
	{
		a[i] = strlen("abcdefghij") ;
	}
	for(i=0 ; i<10 ; i++)
	{
		printf("a[%d] => %d \n",i,a[i]) ;
	}
	exit (0);
}