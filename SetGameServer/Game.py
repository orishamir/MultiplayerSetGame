from itertools import combinations

colors = ['Red', 'Green', 'Purple']
shapes = ['Ellipse', 'Curve', 'Triangle']
fillings = ['Full', 'Empty', 'Stripes']
numbers = ['1', '2', '3']

cards = []

for col in colors:
    for shape in shapes:
        for filling in fillings:
            for num in numbers:
                cards.append(f"{shape}_{num}_{filling}_{col}")


def is_set(cards):
    shape1, amount1, filling1, color1 = cards[0].split("_")
    shape2, amount2, filling2, color2 = cards[1].split("_")
    shape3, amount3, filling3, color3 = cards[2].split("_")

    if shape1 == shape2 == shape3:
        if amount1 == amount2 == amount3:
            if filling1 == filling2 == filling3:
                if color1 == color2 == color3:
                    return True
                elif color1 != color2 and color2 != color3 and color1 != color3:
                    return True
            elif filling1 != filling2 and filling2 != filling3 and filling1 != filling3:
                if color1 == color2 == color3:
                    return True
                elif color1 != color2 and color2 != color3 and color1 != color3:
                    return True
        elif amount1 != amount2 and amount1 != amount3 and amount2 != amount3:
            if filling1 == filling2 == filling3:
                if color1 == color2 == color3:
                    return True
                elif color1 != color2 and color2 != color3 and color1 != color3:
                    return True
            elif filling1 != filling2 and filling2 != filling3 and filling1 != filling3:
                if color1 == color2 == color3:
                    return True
                elif color1 != color2 and color2 != color3 and color1 != color3:
                    return True
    elif shape1 != shape2 and shape1 != shape3 and shape2 != shape3:
        if amount1 == amount2 == amount3:
            if filling1 == filling2 == filling3:
                if color1 == color2 == color3:
                    return True
                elif color1 != color2 and color2 != color3 and color1 != color3:
                    return True
            elif filling1 != filling2 and filling2 != filling3 and filling1 != filling3:
                if color1 == color2 == color3:
                    return True
                elif color1 != color2 and color2 != color3 and color1 != color3:
                    return True
        elif amount1 != amount2 and amount1 != amount3 and amount2 != amount3:
            if filling1 == filling2 == filling3:
                if color1 == color2 == color3:
                    return True
                elif color1 != color2 and color2 != color3 and color1 != color3:
                    return True
            elif filling1 != filling2 and filling2 != filling3 and filling1 != filling3:
                if color1 == color2 == color3:
                    return True
                elif color1 != color2 and color2 != color3 and color1 != color3:
                    return True
    return False


def return_sets(board):
    for comb in combinations(board, 3):
        if is_set(comb):
            yield comb
