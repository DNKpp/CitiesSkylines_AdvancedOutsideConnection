## Author
Dominic Koepke  
Mail: DNKpp2011@gmail.com

## License

[BSL-1.0](https://github.com/DNKpp/CitiesSkylines_AdvancedOutsideConnection/blob/master/LICENSE_1_0.txt) (free, open source)

```
          Copyright Dominic Koepke 2020 - 2021.
 Distributed under the Boost Software License, Version 1.0.
    (See accompanying file LICENSE_1_0.txt or copy at
          https://www.boost.org/LICENSE_1_0.txt)
```

## Description
AdvancedOutsideConnections provides some advanced features for manipulating the ingame outside connections. It offers an dedicated UI, which is attached to the vanilla
OutsideConnectionViewPanel.

## Harmony patching
This mod uses Harmony 1.2.0.1 .

The following patches will be applied:

Class | Function | Prefix | Postfix | Description
------|----------|--------|---------|----------------
OutsideConnectionAI | GenerateName | no | yes | If user applies any of the two custom name settings, Postfix function will **override the returned string**.
OutsideConnectionAI | AddConnectionOffers | yes | no | Alters touristFactorN, cargoCapacity, residentCapacity and dummyTrafficFactor params.
OutsideConnectionAI | StartConnectionTransferImpl | yes | no | Alters touristFactorN params.
TransferManager | AddIncomingOffers | yes | no | Alters material type.
TransferManager | AddOutgoingOffers | yes | no | Alters material type.
Citizen | GetCitizenInfo | yes | no | Only active in debug mode.

