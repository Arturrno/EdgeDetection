;/*************************************************************************************************************
; *  Temat   :   EdgeDetection - Prosty program do detekcji krawedzi w obrazach,
; *              udalo mi sie zaimplementowac algorytm poszarzania i blurowania w obrazach w jezyku C# oraz w jezyku asemblera.
; *  Autor   :   Artur Szabon
; *  Opis    :   Program pozwala na wczytanie obrazu w formacie .bmp, a nastepnie przetworzenie go za pomoca algorytmu przeksztalcenia
; *              na odcienie szarosci, poprzez przemnozenie wartosci pikseli przez odpowiednie wagi. Nastepnie obraz jest rozmywany
; *              poprzez wziecie sredniej z 4 sasiednich pikseli oraz piksela centralnego. Wynikowy obraz jest wyswietlany w oknie.
; *  Wersja  :   V0.5 - implementacja dzialajacego blura w C# oraz w ASM
; *              V0.4 - proby dodania algorytmu blurowania w ASM, dodanie nowego algorytmu w C#
; *              V0.3 - zamiana algorytmu wektorowego na algorytm obslugujacy pojedyncze piksele aby dodac blur, dodanie zapisu wyniku do pliku Excel
; *              V0.2 - dodanie obslugi wielowatkowosci, dodanie obslugi bibliotek DLL, dodanie algorytmu w C#, modyfikacja interfejsu
; *              V0.1 - stworzenie projektu i dodanie bibliotek DLL, implementacja algorytmu wektorowego w ASM, stworzenie prostego interfejsu
; *************************************************************************************************************/

.data
r dd 0.3f 
g dd 0.59f
b dd 0.11f

redWeight dq 0.3f 
greenWeight dq 0.59f
blueWeight dq 0.11f

; Wagi dla poszczegolnych kanalow kolorow na podstawie ludzkiej percepcji

.code
EdgeDetectRGB proc
    ; Rejestry:
    ; rcx - wskaznik do kanalu czerwonego (red channel)
    ; rdx - wskaznik do kanalu zielonego (green channel)
    ; r8 -  wskaznik do kanalu niebieskiego (blue channel)
    ; r9 -  wskaznik do bufora wyjsciowego (output buffer)

    ; Czerwony (Red)
    vmovups xmm0, xmmword ptr [rcx]           ; pobierz 8 bajtow danych (piksele w grupie)
    vpmovzxbd ymm0, xmm0                      ; rozszerz bajty do slow (16-bit)
    vcvtdq2ps ymm0, ymm0                      ; konwertuj dane calkowite na liczby zmiennoprzecinkowe

    ; Zielony (Green)
    vmovups xmm1, xmmword ptr [rdx]           ; pobierz 8 bajtow danych z kanalu zielonego
    vpmovzxbd ymm1, xmm1                      ; rozszerz bajty do slow
    vcvtdq2ps ymm1, ymm1                      ; konwertuj dane calkowite na liczby zmiennoprzecinkowe

    ; Niebieski (Blue)
    vmovups xmm2, xmmword ptr [r8]            ; pobierz 8 bajtow danych z kanalu niebieskiego
    vpmovzxbd ymm2, xmm2                      ; rozszerz bajty do slow
    vcvtdq2ps ymm2, ymm2                      ; konwertuj dane calkowite na liczby zmiennoprzecinkowe

    ; Rozglos wagi do wszystkich elementow rejestru
    vbroadcastss ymm3, r                      ; rozglos wage dla kanalu czerwonego
    vbroadcastss ymm4, g                      ; rozglos wage dla kanalu zielonego
    vbroadcastss ymm5, b                      ; rozglos wage dla kanalu niebieskiego

    ; Pomnoz dane kanalow przez odpowiednie wagi
    vmulps ymm0, ymm0, ymm3                   ; pomnoz kanal czerwony przez wage
    vmulps ymm1, ymm1, ymm4                   ; pomnoz kanal zielony przez wage
    vmulps ymm2, ymm2, ymm5                   ; pomnoz kanal niebieski przez wage

    ; Sumuj wyniki, aby uzyskac wartosc skali szarosci
    vaddps ymm0, ymm0, ymm1                   ; dodaj kanal czerwony i zielony
    vaddps ymm0, ymm0, ymm2                   ; dodaj kanal niebieski

    ; Konwertuj wyniki na wartosci calkowite
    vcvttps2dq ymm0, ymm0                     ; konwertuj liczby zmiennoprzecinkowe na calkowite

    ; Zapisz przetworzone dane do pamieci // xmm0 przechowuje 16 bajtow, czyli 128 bitow
    pextrb byte ptr [r9+0], xmm0, 0           ; wyciagnij i zapisz pierwszy bajt
    pextrb byte ptr [r9+1], xmm0, 4           ; wyciagnij i zapisz drugi bajt
    pextrb byte ptr [r9+2], xmm0, 8           ; wyciagnij i zapisz trzeci bajt
    pextrb byte ptr [r9+3], xmm0, 12          ; wyciagnij i zapisz czwarty bajt

    ; Przestaw dane w rejestrze ymm0, aby obsluzyc kolejne piksele
    vpermq ymm0, ymm0, 11111110b              ; zamien dolne i gorne czesci rejestru (na poziomie 128-bitow), poniewaz xmm to polowa ymm

    ; Zapisz przetworzone dane dla kolejnych pikseli
    pextrb byte ptr [r9+4], xmm0, 0           ; wyciagnij i zapisz piaty bajt
    pextrb byte ptr [r9+5], xmm0, 4           ; wyciagnij i zapisz szosty bajt
    pextrb byte ptr [r9+6], xmm0, 8           ; wyciagnij i zapisz siodmy bajt
    pextrb byte ptr [r9+7], xmm0, 12          ; wyciagnij i zapisz osmy bajt
    
    ; Koniec procedury
    ret
EdgeDetectRGB endp

    ; /////////////////////////////////////////////////////////////////////////////////////////

EdgeDetect proc
        ; rcx - wskaznik do bufora wejsciowego (input buffer)
        ; rdx - wskaznik do bufora wyjsciowego (output buffer)
        ; r8 - szerokosc (width)
        ; r9 - wysokosc (height)

        ; Zapisz rejestry na stosie
        push rbp
        push rbx
        push rsi
        push rdi
        push r12
        push r13
        push r14
        push r15

        ; Przekonwertuj obraz na skale szarosci
        call Grayscale

        ; Wykonaj rozmycie na obrazie w skali szarosci
        call Blur

        ; Przywroc rejestry ze stosu
        pop r15
        pop r14
        pop r13
        pop r12
        pop rdi
        pop rsi
        pop rbx
        pop rbp

        ret

Grayscale:
        ; Przekonwertuj obraz na skale szarosci
        ; rcx - wskaznik do bufora wejsciowego (input buffer)
        ; rdx - wskaznik do bufora wyjsciowego (output buffer)
        ; r8 - szerokosc (width)
        ; r9 - wysokosc (height)

        ; Oblicz calkowita liczbe pikseli (szerokosc * wysokosc * 3)
        imul r9, r8
        imul r9, 3
        mov r13, r9

        xor rax, rax   ; rax = aktualny indeks piksela

    processLoop:
        cmp rax, r13
        jge doneGrayscale

        ; Wczytaj wartosci RGB
        movzx r10, byte ptr [rcx + rax]         ; R
        movzx r11, byte ptr [rcx + rax + 1]     ; G
        movzx r12, byte ptr [rcx + rax + 2]     ; B

        ; Przekonwertuj na skale szarosci uzywajac wazonej sumy
        cvtsi2sd xmm0, r10                      ; R na liczbe zmiennoprzecinkowa
        mulsd xmm0, qword ptr [redWeight]       ; pomnoz przez wage

        cvtsi2sd xmm1, r11                      
        mulsd xmm1, qword ptr [greenWeight]     ; pomnoz przez wage

        cvtsi2sd xmm2, r12                      
        mulsd xmm2, qword ptr [blueWeight]      ; pomnoz przez wage

        addsd xmm0, xmm1                        ; R + G
        addsd xmm0, xmm2                        ; R + G + B

        ; Przekonwertuj z powrotem na liczbe calkowita
        cvttsd2si r10d, xmm0                    ; Konwertuj na liczbe calkowita

        ; Zapisz wartosc w skali szarosci w buforze wyjsciowym
        mov byte ptr [rdx + rax], r10b          ; R
        mov byte ptr [rdx + rax + 1], r10b      ; G
        mov byte ptr [rdx + rax + 2], r10b      ; B

        ; Zapisz wartosc w skali szarosci w buforze wejsciowym (opcjonalnie)
        mov byte ptr [rcx + rax], r10b          ; R
        mov byte ptr [rcx + rax + 1], r10b      ; G
        mov byte ptr [rcx + rax + 2], r10b      ; B

        ; Przejdz do nastepnego piksela
        add rax, 3
        jmp processLoop

    doneGrayscale:
        ret

    Blur:
        ; Rozmycie obrazu w skali szarosci
        ; rcx - wskaznik do bufora wejsciowego (input buffer)
        ; rdx - wskaznik do bufora wyjsciowego (output buffer)
        ; r8 - szerokosc (width)
        ; r9 - wysokosc (height)

        ; Obliczenie liczby bajtow na wiersz (stride)
        mov r10, r8
        imul r10, 3  ; r10 = szerokosc * 3 (bajty na wiersz)

        ; Inicjalizacja licznikow petli
        mov r11, r9  ; r11 = wysokosc (limit petli zewnetrznej)
        sub r11, 1   ; Pominiecie pikseli brzegowych (1 piksel od gory i dolu)

        mov r12, r10  ; r12 = szerokosc (limit petli wewnetrznej)
        sub r12, 3   ; Pominiecie pikseli brzegowych (3 piksele od lewej i prawej)

        ; Inicjalizacja indeksu piksela
        xor rsi, rsi ; rsi = aktualny indeks wiersza (w bajtach)
        add rsi, 1   ; Pominiecie pierwszego wiersza

    outerLoop:
        cmp rsi, r11
        jge doneBlur

        ; Inicjalizacja licznika petli wewnetrznej
        xor rdi, rdi ; rdi = aktualny indeks kolumny (w bajtach)
        add rdi, 3   ; Pominiecie pierwszych 3 kolumn

    innerLoop:
        cmp rdi, r12
        jge nextRow

        ; Obliczenie sumy sasiedztwa 5 pikseli
        xor r13, r13 ; r13 = suma sasiedztwa

        ; Piksel srodkowy
        mov r14, rsi
        add r14, rdi
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Piksel gorny
        mov r14, rsi
        add r14, rdi
        sub r14, r10
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Piksel dolny
        mov r14, rsi
        add r14, rdi
        add r14, r10
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Piksel lewy
        mov r14, rsi
        add r14, rdi
        sub r14, 3
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Piksel prawy
        mov r14, rsi
        add r14, rdi
        add r14, 3
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Zapisanie rcx i rdx przed dzieleniem
        push rcx
        push rdx

        ; Przygotowanie dzielnej
        mov rax, r13        ; Przeniesienie r13 (wartosc do podzielenia) do rax
        xor rdx, rdx        ; Wyczyszczenie rdx (wysokie 64 bity dzielnej)

        ; Obliczenie sredniej (podzial przez 5)
        mov r14, 5          ; Dzielnik (5)
        div r14             ; Dzielenie (rdx:rax) przez r14
        mov r13, rax        ; Iloraz jest teraz w r13 = rax, reszta w rdx

        ; Przywrocenie oryginalnych wartosci rcx i rdx
        pop rdx
        pop rcx

        ; Zapisanie rozmytego piksela do bufora wyjsciowego
        mov r14, rsi
        add r14, rdi
        mov byte ptr [rdx + r14], r13b
        mov byte ptr [rdx + r14 + 1], r13b
        mov byte ptr [rdx + r14 + 2], r13b

        ; Przejscie do nastepnej kolumny
        add rdi, 3
        jmp innerLoop

    nextRow:
        ; Przejscie do nastepnego wiersza
        add rsi, r10
        jmp outerLoop

    doneBlur:
        ret

EdgeDetect endp

end