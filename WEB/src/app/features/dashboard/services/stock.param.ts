import { inject, Injectable, signal } from "@angular/core";
import { StockOptionViewModel } from "../models/stock.model";
import { StockApiService } from "./stock.api";

@Injectable({
    providedIn: 'root'
})
export class StockParamService {
    private stockApiService = inject(StockApiService);

    stockOptions = signal<StockOptionViewModel[]>([]);



    constructor() {
        this.getStockOptions();
    }
    
    getStockOptions(): void {
        this.stockApiService.getStockOptions().subscribe(data => {
            this.stockOptions.set(data);
        })
    }

}