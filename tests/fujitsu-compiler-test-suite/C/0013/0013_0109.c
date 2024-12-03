#include <stdio.h>                                              

struct ASIB03JB03HB00 {
	unsigned char m1:3;
	unsigned char   :3;
	unsigned char m3;
} asib03jb03hb00 = { 1, 2 };

struct ASIB03JB03IB03 {
	unsigned char m1:3;
	unsigned char   :3;
	unsigned char m3:3;
} asib03jb03ib03 = { 1, 2 };

struct ASIB03JB03IB05 {
	unsigned char m1:3;
	unsigned char   :3;
	unsigned char m3:5;
} asib03jb03ib05 = { 1, 2 };

struct ASIB03JB03HH00 {
	unsigned char m1:3;
	unsigned char   :3;
	unsigned short m3;
} asib03jb03hh00 = { 1, 2 };

struct ASIB03JB03IH03 {
	unsigned char m1:3;
	unsigned char   :3;
	unsigned short m3:3;
} asib03jb03ih03 = { 1, 2 };

struct ASIB03JB03IH13 {
	unsigned char m1:3;
	unsigned char   :3;
	unsigned short m3:13;
} asib03jb03ih13 = { 1, 2 };

struct ASIB03JB03HW00 {
	unsigned char m1:3;
	unsigned char   :3;
	unsigned long m3;
} asib03jb03hw00 = { 1, 2 };

struct ASIB03JB03IW03 {
	unsigned char m1:3;
	unsigned char   :3;
	unsigned long m3:3;
} asib03jb03iw03 = { 1, 2 };

struct ASIB03JB03IW29 {
	unsigned char m1:3;
	unsigned char   :3;
	unsigned long m3:29;
} asib03jb03iw29 = { 1, 2 };

struct ASIB03JB03HD00 {
	unsigned char m1:3;
	unsigned char   :3;
	unsigned long long m3;
} asib03jb03hd00 = { 1, 2 };

struct ASIB03JB03ID03 {
	unsigned char m1:3;
	unsigned char   :3;
	unsigned long long m3:3;
} asib03jb03id03 = { 1, 2 };

struct ASIB03JB03ID61 {
	unsigned char m1:3;
	unsigned char   :3;
	unsigned long long m3:61;
} asib03jb03id61 = { 1, 2 };

struct TAGHB00 {
	unsigned char m1;
};

struct ASIB03JB03HS00 {
	unsigned char m1:3;
	unsigned char   :3;
	struct TAGHB00 m3;
} asib03jb03hs00 = { 1, { 2 } };

int main() {

	printf("%u\n"  , asib03jb03hb00.m1);
	printf("%u\n"  , asib03jb03hb00.m3);
	printf("\n");

	printf("%u\n"  , asib03jb03ib03.m1);
	printf("%u\n"  , asib03jb03ib03.m3);
	printf("\n");

	printf("%u\n"  , asib03jb03ib05.m1);
	printf("%u\n"  , asib03jb03ib05.m3);
	printf("\n");

	printf("%u\n"  , asib03jb03hh00.m1);
	printf("%u\n"  , asib03jb03hh00.m3);
	printf("\n");

	printf("%u\n"  , asib03jb03ih03.m1);
	printf("%u\n"  , asib03jb03ih03.m3);
	printf("\n");

	printf("%u\n"  , asib03jb03ih13.m1);
	printf("%u\n"  , asib03jb03ih13.m3);
	printf("\n");

	printf("%u\n"  , asib03jb03hw00.m1);
	printf("%lu\n" , asib03jb03hw00.m3);
	printf("\n");

	printf("%u\n"  , asib03jb03iw03.m1);
	printf("%d\n" , asib03jb03iw03.m3);
	printf("\n");

	printf("%u\n"  , asib03jb03iw29.m1);
	printf("%d\n" , asib03jb03iw29.m3);
	printf("\n");

	printf("%u\n"  , asib03jb03hd00.m1);
	printf("%llu\n", asib03jb03hd00.m3);
	printf("\n");

	printf("%u\n"  , asib03jb03id03.m1);
	printf("%llu\n", (unsigned long long)asib03jb03id03.m3);
	printf("\n");

	printf("%u\n"  , asib03jb03id61.m1);
	printf("%llu\n", (unsigned long long)asib03jb03id61.m3);
	printf("\n");

	printf("%u\n"  , asib03jb03hs00.m1);
	printf("%u\n"  , asib03jb03hs00.m3.m1);
	printf("\n");

	return 0;
}
