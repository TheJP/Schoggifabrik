main = do
    (a:b:vs) <- (fmap words getContents)
    let (w, h, values) = (readInt a, readInt b, fmap readInt vs)
    print w
    print h


readInt :: String -> Int
readInt = read