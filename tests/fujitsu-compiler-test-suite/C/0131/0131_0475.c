#include <stdlib.h>
#include <stdio.h>
#include <math.h>

int main()
{
	int i=0,a[5];
	for(;i<5;i++)
	{
		a[i] = i ;
	}
	printf(" a[0:4] => %d %d %d %d %d\n",a[0],a[1],a[2],a[3],a[4]) ;
	exit (0);
}
