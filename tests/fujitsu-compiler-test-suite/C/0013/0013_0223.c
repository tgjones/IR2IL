#include <stdio.h>                                              

struct ASIW03ID61HB00 {
	unsigned long m1:3;
	unsigned long long m2:61;
	unsigned char m3;
} asiw03id61hb00 = { 1, 2, 3 };

struct ASIW03ID61IB03 {
	unsigned long m1:3;
	unsigned long long m2:61;
	unsigned char m3:3;
} asiw03id61ib03 = { 1, 2, 3 };

struct ASIW03ID61IB05 {
	unsigned long m1:3;
	unsigned long long m2:61;
	unsigned char m3:5;
} asiw03id61ib05 = { 1, 2, 3 };

struct ASIW03ID61HH00 {
	unsigned long m1:3;
	unsigned long long m2:61;
	unsigned short m3;
} asiw03id61hh00 = { 1, 2, 3 };

struct ASIW03ID61IH03 {
	unsigned long m1:3;
	unsigned long long m2:61;
	unsigned short m3:3;
} asiw03id61ih03 = { 1, 2, 3 };

struct ASIW03ID61IH13 {
	unsigned long m1:3;
	unsigned long long m2:61;
	unsigned short m3:13;
} asiw03id61ih13 = { 1, 2, 3 };

struct ASIW03ID61HW00 {
	unsigned long m1:3;
	unsigned long long m2:61;
	unsigned long m3;
} asiw03id61hw00 = { 1, 2, 3 };

struct ASIW03ID61IW03 {
	unsigned long m1:3;
	unsigned long long m2:61;
	unsigned long m3:3;
} asiw03id61iw03 = { 1, 2, 3 };

struct ASIW03ID61IW29 {
	unsigned long m1:3;
	unsigned long long m2:61;
	unsigned long m3:29;
} asiw03id61iw29 = { 1, 2, 3 };

struct ASIW03ID61HD00 {
	unsigned long m1:3;
	unsigned long long m2:61;
	unsigned long long m3;
} asiw03id61hd00 = { 1, 2, 3 };

struct ASIW03ID61ID03 {
	unsigned long m1:3;
	unsigned long long m2:61;
	unsigned long long m3:3;
} asiw03id61id03 = { 1, 2, 3 };

struct ASIW03ID61ID61 {
	unsigned long m1:3;
	unsigned long long m2:61;
	unsigned long long m3:61;
} asiw03id61id61 = { 1, 2, 3 };

struct TAGHB00 {
	unsigned char m1;
};

struct ASIW03ID61HS00 {
	unsigned long m1:3;
	unsigned long long m2:61;
	struct TAGHB00 m3;
} asiw03id61hs00 = { 1, 2, { 3 } };

int main() {

	printf("%d\n" , asiw03id61hb00.m1);
	printf("%llu\n", (unsigned long long)asiw03id61hb00.m2);
	printf("%u\n"  , asiw03id61hb00.m3);
	printf("\n");

	printf("%d\n" , asiw03id61ib03.m1);
	printf("%llu\n", (unsigned long long)asiw03id61ib03.m2);
	printf("%u\n"  , asiw03id61ib03.m3);
	printf("\n");

	printf("%d\n" , asiw03id61ib05.m1);
	printf("%llu\n", (unsigned long long)asiw03id61ib05.m2);
	printf("%u\n"  , asiw03id61ib05.m3);
	printf("\n");

	printf("%d\n" , asiw03id61hh00.m1);
	printf("%llu\n", (unsigned long long)asiw03id61hh00.m2);
	printf("%u\n"  , asiw03id61hh00.m3);
	printf("\n");

	printf("%d\n" , asiw03id61ih03.m1);
	printf("%llu\n", (unsigned long long)asiw03id61ih03.m2);
	printf("%u\n"  , asiw03id61ih03.m3);
	printf("\n");

	printf("%d\n" , asiw03id61ih13.m1);
	printf("%llu\n", (unsigned long long)asiw03id61ih13.m2);
	printf("%u\n"  , asiw03id61ih13.m3);
	printf("\n");

	printf("%d\n" , asiw03id61hw00.m1);
	printf("%llu\n", (unsigned long long)asiw03id61hw00.m2);
	printf("%lu\n" , asiw03id61hw00.m3);
	printf("\n");

	printf("%d\n" , asiw03id61iw03.m1);
	printf("%llu\n", (unsigned long long)asiw03id61iw03.m2);
	printf("%d\n" , asiw03id61iw03.m3);
	printf("\n");

	printf("%d\n" , asiw03id61iw29.m1);
	printf("%llu\n", (unsigned long long)asiw03id61iw29.m2);
	printf("%d\n" , asiw03id61iw29.m3);
	printf("\n");

	printf("%d\n" , asiw03id61hd00.m1);
	printf("%llu\n", (unsigned long long)asiw03id61hd00.m2);
	printf("%llu\n", asiw03id61hd00.m3);
	printf("\n");

	printf("%d\n" , asiw03id61id03.m1);
	printf("%llu\n", (unsigned long long)asiw03id61id03.m2);
	printf("%llu\n", (unsigned long long)asiw03id61id03.m3);
	printf("\n");

	printf("%d\n" , asiw03id61id61.m1);
	printf("%llu\n", (unsigned long long)asiw03id61id61.m2);
	printf("%llu\n", (unsigned long long)asiw03id61id61.m3);
	printf("\n");

	printf("%d\n" , asiw03id61hs00.m1);
	printf("%llu\n", (unsigned long long)asiw03id61hs00.m2);
	printf("%u\n"  , asiw03id61hs00.m3.m1);
	printf("\n");

	return 0;
}
