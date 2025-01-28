.data
r DD 0.3f               ; Waga dla kana�u czerwonego (czerwona sk�adowa ma wp�yw w 30%)
g DD 0.59f              ; Waga dla kana�u zielonego (zielona sk�adowa ma wp�yw w 59%)
b DD 0.11f              ; Waga dla kana�u niebieskiego (niebieska sk�adowa ma wp�yw w 11%)

.code
EdgeDetect proc
; Procedura
; Konwertuje piksele obrazu na odcienie szaro�ci z u�yciem wag kolor�w.
; Rejestry wej�ciowe:
; - RCX: wska�nik na kana� czerwony
; - RDX: wska�nik na kana� zielony
; - R8:  wska�nik na kana� niebieski

    ; �adowanie danych kana�u czerwonego do xmm0
    VMOVUPS xmm0, xmmword ptr [RCX]           ; Pobierz 8 bajt�w danych (piksele) z pami�ci wskazanej przez RCX
    VPMOVZXBD ymm0, xmm0                      ; Rozszerz bajty do s��w (16-bit�w) z wype�nieniem zerami
    VCVTDQ2PS ymm0, ymm0                      ; Konwertuj dane ca�kowite na liczby zmiennoprzecinkowe (float)

    ; �adowanie danych kana�u zielonego do xmm1
    VMOVUPS xmm1, xmmword ptr [RDX]           ; Pobierz 8 bajt�w danych (piksele) z pami�ci wskazanej przez RDX
    VPMOVZXBD ymm1, xmm1                      ; Rozszerz bajty do s��w (16-bit�w)
    VCVTDQ2PS ymm1, ymm1                      ; Konwertuj dane ca�kowite na liczby zmiennoprzecinkowe (float)

    ; �adowanie danych kana�u niebieskiego do xmm2
    VMOVUPS xmm2, xmmword ptr [R8]            ; Pobierz 8 bajt�w danych (piksele) z pami�ci wskazanej przez R8
    VPMOVZXBD ymm2, xmm2                      ; Rozszerz bajty do s��w (16-bit�w)
    VCVTDQ2PS ymm2, ymm2                      ; Konwertuj dane ca�kowite na liczby zmiennoprzecinkowe (float)

    ; Za�adowanie wag kana��w kolor�w
    VBROADCASTSS ymm3, r                      ; Powiel wag� czerwonego kana�u w ca�ym rejestrze ymm3
    VBROADCASTSS ymm4, g                      ; Powiel wag� zielonego kana�u w ca�ym rejestrze ymm4
    VBROADCASTSS ymm5, b                      ; Powiel wag� niebieskiego kana�u w ca�ym rejestrze ymm5

    ; Mno�enie danych kana��w przez odpowiednie wagi
    VMULPS ymm0, ymm0, ymm3                   ; Mno�enie kana�u czerwonego przez jego wag�
    VMULPS ymm1, ymm1, ymm4                   ; Mno�enie kana�u zielonego przez jego wag�
    VMULPS ymm2, ymm2, ymm5                   ; Mno�enie kana�u niebieskiego przez jego wag�

    ; Sumowanie wynik�w w celu uzyskania warto�ci odcienia szaro�ci
    VADDPS ymm0, ymm0, ymm1                   ; Dodanie warto�ci kana�u zielonego do kana�u czerwonego
    VADDPS ymm0, ymm0, ymm2                   ; Dodanie warto�ci kana�u niebieskiego do sumy (pe�ny odcie� szaro�ci)

    ; Konwersja wynik�w na liczby ca�kowite
    VCVTTPS2DQ ymm0, ymm0                     ; Konwersja z liczb zmiennoprzecinkowych na ca�kowite (zaokr�glenie w d�)

    ; Zapis przetworzonych danych do pami�ci
    pextrb byte ptr [R9+0], xmm0, 0          ; Zapisanie warto�ci dla pierwszego piksela (czerwony)
    pextrb byte ptr [R9+1], xmm0, 4          ; Zapisanie warto�ci dla pierwszego piksela (zielony)
    pextrb byte ptr [R9+2], xmm0, 8          ; Zapisanie warto�ci dla pierwszego piksela (niebieski)
    pextrb byte ptr [R9+3], xmm0, 12         ; Zapisanie warto�ci dla drugiego piksela (czerwony)

    ; Przeorganizowanie danych w rejestrze ymm0, by obs�u�y� kolejne piksele
    VPERMQ ymm0, ymm0, 11111110b              ; Zamiana dolnej i g�rnej cz�ci rejestru (na poziomie 128 bit�w)

    ; Zapis przetworzonych danych dla kolejnych pikseli
    pextrb byte ptr [R9+4], xmm0, 0          ; Zapisanie warto�ci dla trzeciego piksela (czerwony)
    pextrb byte ptr [R9+5], xmm0, 4          ; Zapisanie warto�ci dla trzeciego piksela (zielony)
    pextrb byte ptr [R9+6], xmm0, 8          ; Zapisanie warto�ci dla trzeciego piksela (niebieski)
    pextrb byte ptr [R9+7], xmm0, 12         ; Zapisanie warto�ci dla czwartego piksela (czerwony)

    ; Zako�czenie procedury
    ret                                       ; Powr�t do miejsca wywo�ania

EdgeDetect endp
end
